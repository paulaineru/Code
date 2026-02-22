<?php

namespace App\Controllers\Api;

use App\Models\ApplicationModel;
use App\Models\SatSummaryModel;
use App\Models\SatMonthlyModel;
use App\Models\SatArrearsModel;

class SatData extends BaseApi
{
    protected ApplicationModel $applicationModel;
    protected SatSummaryModel $satSummaryModel;
    protected SatMonthlyModel $satMonthlyModel;
    protected SatArrearsModel $satArrearsModel;

    public function __construct()
    {
        parent::__construct();
        $this->applicationModel = new ApplicationModel();
        $this->satSummaryModel = new SatSummaryModel();
        $this->satMonthlyModel = new SatMonthlyModel();
        $this->satArrearsModel = new SatArrearsModel();
    }

    /**
     * GET /api/applications/{id}/sat-summary - Get SAT summary data
     */
    public function summary($id = null)
    {
        if (!$this->verifyApiKey()) {
            return $this->unauthorized();
        }

        $application = $this->applicationModel->find($id);

        if (!$application) {
            return $this->notFound('Application not found');
        }

        $summary = $this->satSummaryModel->getByApplicationId($id);

        if (!$summary) {
            return $this->notFound('SAT summary not found for this application');
        }

        return $this->success($summary);
    }

    /**
     * GET /api/applications/{id}/sat-monthly - Get monthly totals data
     */
    public function monthly($id = null)
    {
        if (!$this->verifyApiKey()) {
            return $this->unauthorized();
        }

        $application = $this->applicationModel->find($id);

        if (!$application) {
            return $this->notFound('Application not found');
        }

        $monthly = $this->satMonthlyModel->getByApplicationId($id);

        return $this->success([
            'application_id' => $id,
            'count'          => count($monthly),
            'records'        => $monthly
        ]);
    }

    /**
     * GET /api/applications/{id}/sat-arrears - Get employee arrears data
     */
    public function arrears($id = null)
    {
        if (!$this->verifyApiKey()) {
            return $this->unauthorized();
        }

        $application = $this->applicationModel->find($id);

        if (!$application) {
            return $this->notFound('Application not found');
        }

        $page = (int) ($this->request->getGet('page') ?? 1);
        $limit = (int) ($this->request->getGet('limit') ?? 100);

        $arrears = $this->satArrearsModel->getByApplicationId($id);
        $total = count($arrears);

        // Manual pagination for simplicity
        $offset = ($page - 1) * $limit;
        $paginatedArrears = array_slice($arrears, $offset, $limit);

        return $this->success([
            'application_id' => $id,
            'pagination'     => [
                'page'       => $page,
                'limit'      => $limit,
                'total'      => $total,
                'totalPages' => ceil($total / $limit),
            ],
            'records' => $paginatedArrears
        ]);
    }
}
