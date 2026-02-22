<?php

namespace App\Controllers\Api;

use App\Models\ApplicationModel;
use App\Models\SatSummaryModel;
use App\Models\SatMonthlyModel;
use App\Models\SatArrearsModel;
use App\Models\SyncLogModel;

class Applications extends BaseApi
{
    protected ApplicationModel $applicationModel;
    protected SatSummaryModel $satSummaryModel;
    protected SatMonthlyModel $satMonthlyModel;
    protected SatArrearsModel $satArrearsModel;
    protected SyncLogModel $syncLogModel;

    public function __construct()
    {
        parent::__construct();
        $this->applicationModel = new ApplicationModel();
        $this->satSummaryModel = new SatSummaryModel();
        $this->satMonthlyModel = new SatMonthlyModel();
        $this->satArrearsModel = new SatArrearsModel();
        $this->syncLogModel = new SyncLogModel();
    }

    /**
     * GET /api/applications - List all applications
     */
    public function index()
    {
        if (!$this->verifyApiKey()) {
            return $this->unauthorized();
        }

        $page = (int) ($this->request->getGet('page') ?? 1);
        $limit = (int) ($this->request->getGet('limit') ?? 20);
        $offset = ($page - 1) * $limit;

        $filters = [
            'status'          => $this->request->getGet('status'),
            'employer_number' => $this->request->getGet('employer_number'),
            'from_date'       => $this->request->getGet('from_date'),
            'to_date'         => $this->request->getGet('to_date'),
        ];

        $result = $this->applicationModel->getApplications($filters, $limit, $offset);

        return $this->success([
            'applications' => $result['data'],
            'pagination'   => [
                'page'       => $page,
                'limit'      => $limit,
                'total'      => $result['total'],
                'totalPages' => ceil($result['total'] / $limit),
            ]
        ]);
    }

    /**
     * GET /api/applications/{id} - Get single application
     */
    public function show($id = null)
    {
        if (!$this->verifyApiKey()) {
            return $this->unauthorized();
        }

        $application = $this->applicationModel->find($id);

        if (!$application) {
            return $this->notFound('Application not found');
        }

        // Include related data
        $application['sat_summary'] = $this->satSummaryModel->getByApplicationId($id);
        $application['sync_logs'] = $this->syncLogModel->getByApplicationId($id);

        return $this->success($application);
    }

    /**
     * PUT /api/applications/{id}/status - Update application status
     */
    public function updateStatus($id = null)
    {
        if (!$this->verifyApiKey()) {
            return $this->unauthorized();
        }

        $application = $this->applicationModel->find($id);

        if (!$application) {
            return $this->notFound('Application not found');
        }

        $data = $this->request->getJSON(true);
        $newStatus = $data['status'] ?? null;
        $rejectionReason = $data['rejection_reason'] ?? null;

        $validStatuses = ['submitted', 'under_review', 'accepted', 'rejected'];
        if (!in_array($newStatus, $validStatuses)) {
            return $this->error('Invalid status. Valid values: ' . implode(', ', $validStatuses));
        }

        // Prepare update data
        $updateData = ['status' => $newStatus];

        // Handle rejection_reason based on status
        if ($newStatus === 'rejected') {
            if (empty($rejectionReason)) {
                return $this->error('rejection_reason is required when status is rejected');
            }
            $updateData['rejection_reason'] = $rejectionReason;
        } else {
            // Clear rejection_reason when status is not rejected
            $updateData['rejection_reason'] = null;
        }

        $this->applicationModel->update($id, $updateData);

        $responseData = [
            'id'     => $id,
            'status' => $newStatus
        ];

        if ($newStatus === 'rejected') {
            $responseData['rejection_reason'] = $rejectionReason;
        }

        return $this->success($responseData, 'Status updated successfully');
    }

    /**
     * GET /api/applications/{id}/files/{type} - Download file
     */
    public function downloadFile($id = null, $type = null)
    {
        if (!$this->verifyApiKey()) {
            return $this->unauthorized();
        }

        $application = $this->applicationModel->find($id);

        if (!$application) {
            return $this->notFound('Application not found');
        }

        $filePath = null;
        $fileName = null;

        switch ($type) {
            case 'sat':
                $filePath = $application['sat_file_path'];
                $fileName = 'SAT_' . $application['employer_number'] . '.xlsm';
                break;
            case 'payment':
                $filePath = $application['payment_proof_path'];
                $ext = pathinfo($filePath, PATHINFO_EXTENSION);
                $fileName = 'PaymentProof_' . $application['employer_number'] . '.' . $ext;
                break;
            default:
                return $this->error('Invalid file type. Valid values: sat, payment');
        }

        if (!$filePath || !file_exists($filePath)) {
            return $this->notFound('File not found');
        }

        return $this->response->download($filePath, null)->setFileName($fileName);
    }

    /**
     * POST /api/sync/{id} - Trigger sync for application
     */
    public function triggerSync($id = null)
    {
        if (!$this->verifyApiKey()) {
            return $this->unauthorized();
        }

        $application = $this->applicationModel->find($id);

        if (!$application) {
            return $this->notFound('Application not found');
        }

        // Reset sync status to pending
        $syncLogs = $this->syncLogModel->getByApplicationId($id);

        foreach ($syncLogs as $log) {
            $this->syncLogModel->update($log['id'], [
                'status'   => 'pending',
                'attempts' => 0,
            ]);
        }

        return $this->success([
            'id'      => $id,
            'message' => 'Sync queued for processing'
        ]);
    }
}
