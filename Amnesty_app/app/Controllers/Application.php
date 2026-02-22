<?php

namespace App\Controllers;

use App\Models\ApplicationModel;
use App\Models\SatSummaryModel;
use App\Models\SatMonthlyModel;
use App\Models\SatArrearsModel;
use App\Models\SyncLogModel;
use App\Libraries\SatParser;

class Application extends BaseController
{
    protected ApplicationModel $applicationModel;
    protected SatSummaryModel $satSummaryModel;
    protected SatMonthlyModel $satMonthlyModel;
    protected SatArrearsModel $satArrearsModel;
    protected SyncLogModel $syncLogModel;

    public function __construct()
    {
        $this->applicationModel = new ApplicationModel();
        $this->satSummaryModel = new SatSummaryModel();
        $this->satMonthlyModel = new SatMonthlyModel();
        $this->satArrearsModel = new SatArrearsModel();
        $this->syncLogModel = new SyncLogModel();
    }

    /**
     * Show application form
     */
    public function form(): string
    {
        // Check if terms are accepted
        if (!session()->get('terms_accepted')) {
            return redirect()->to('/')->with('error', 'Please accept the terms and conditions first.');
        }

        // Sectors list
        $sectors = [
            'Agriculture, Forestry and Fishing',
            'Education',
            'Financial and Insurance Services',
            'Human Health and Social Work Activities',
            'Information, Communication & Technology',
            'Manufacturing and Mining',
            'NGOs, Political & Trade Unions',
            'Professional, Scientific and Technical Activities',
            'Public Administration & Security Companies',
            'Recreation, Accommodation and Food Service Activities',
            'Trade',
            'Transport, Storage, Real Estate and Construction',
            'Utilities and Energy',
            'Voluntary Individuals',
        ];

        return view('application/form', [
            'title'   => 'Submit Application - NSSF Amnesty',
            'sectors' => $sectors,
        ]);
    }

    /**
     * Handle SAT file upload (AJAX)
     */
    public function uploadSat()
    {
        if (!$this->request->isAJAX()) {
            return $this->response->setStatusCode(400)->setJSON(['error' => 'Invalid request']);
        }

        $file = $this->request->getFile('sat_file');

        if (!$file || !$file->isValid()) {
            return $this->response->setJSON([
                'success' => false,
                'error'   => 'Please select a valid file to upload'
            ]);
        }

        // Validate file extension
        $extension = strtolower($file->getExtension());
        if (!in_array($extension, ['xlsm', 'xlsx', 'xls'])) {
            return $this->response->setJSON([
                'success' => false,
                'error'   => 'Invalid file type. Please upload an Excel file (.xlsm, .xlsx, .xls)'
            ]);
        }

        // Move file to temp location
        $newName = $file->getRandomName();
        $file->move(WRITEPATH . 'uploads/temp', $newName);
        $filePath = WRITEPATH . 'uploads/temp/' . $newName;

        // Parse the SAT file
        $parser = new SatParser();
        if (!$parser->parse($filePath)) {
            // Clean up file on error
            @unlink($filePath);

            return $this->response->setJSON([
                'success' => false,
                'errors'  => $parser->getErrors()
            ]);
        }

        // Store parsed data in session
        session()->set('sat_file_path', $filePath);
        session()->set('sat_file_name', $newName);
        session()->set('sat_parsed_data', $parser->getParsedData());

        $summary = $parser->getSummary();
        $totals = $parser->getTotals();

        return $this->response->setJSON([
            'success' => true,
            'data'    => [
                'employer_name'    => $summary['employer_name'] ?? '',
                'employer_number'  => $summary['employer_number'] ?? '',
                'period_from'      => $summary['start_date'] ?? '',
                'period_to'        => $summary['end_date'] ?? '',
                'standard_arrears' => round($totals['standard_arrears'], 2),
                'interest'         => round($totals['interest'], 2),
                'penalty'          => round($totals['penalty'], 2),
                'total_amount'     => round($totals['total_amount'], 2),
                'monthly_count'    => count($parser->getMonthly()),
                'arrears_count'    => count($parser->getArrears()),
            ]
        ]);
    }

    /**
     * Handle payment proof upload (AJAX)
     */
    public function uploadPayment()
    {
        if (!$this->request->isAJAX()) {
            return $this->response->setStatusCode(400)->setJSON(['error' => 'Invalid request']);
        }

        $file = $this->request->getFile('payment_proof');

        if (!$file || !$file->isValid()) {
            return $this->response->setJSON([
                'success' => false,
                'error'   => 'Please select a valid file to upload'
            ]);
        }

        // Validate file type
        $extension = strtolower($file->getExtension());
        if (!in_array($extension, ['pdf', 'jpg', 'jpeg', 'png'])) {
            return $this->response->setJSON([
                'success' => false,
                'error'   => 'Invalid file type. Please upload a PDF or image file (PDF, JPG, PNG)'
            ]);
        }

        // Move file to temp location
        $newName = $file->getRandomName();
        $file->move(WRITEPATH . 'uploads/temp', $newName);

        // Store in session
        session()->set('payment_proof_path', WRITEPATH . 'uploads/temp/' . $newName);
        session()->set('payment_proof_name', $newName);

        return $this->response->setJSON([
            'success'  => true,
            'filename' => $file->getClientName()
        ]);
    }

    /**
     * Submit application (AJAX)
     */
    public function submit()
    {
        if (!$this->request->isAJAX()) {
            return $this->response->setStatusCode(400)->setJSON(['error' => 'Invalid request']);
        }

        // Get form data
        $data = $this->request->getJSON(true);

        // Validate required fields
        $validation = \Config\Services::validation();
        $validation->setRules([
            'employer_number'      => 'required|max_length[50]',
            'employer_name'        => 'required|max_length[255]',
            'email'                => 'required|valid_email|max_length[255]',
            'phone'                => 'required|max_length[20]',
            'sector'               => 'required|max_length[100]',
            'payment_reference'    => 'required|max_length[100]',
            'standard_arrears'     => 'required|numeric',
            'interest'             => 'required|numeric',
            'amount_paid_standard' => 'required|numeric',
            'amount_paid_interest' => 'required|numeric',
        ]);

        if (!$validation->run($data)) {
            return $this->response->setJSON([
                'success' => false,
                'errors'  => $validation->getErrors()
            ]);
        }

        // Validate that amounts paid match the arrears
        $standardArrears = $this->parseNumber($data['standard_arrears']);
        $amountPaidStandard = $this->parseNumber($data['amount_paid_standard']);
        $interest = $this->parseNumber($data['interest']);
        $amountPaidInterest = $this->parseNumber($data['amount_paid_interest']);

        if (abs($amountPaidStandard - $standardArrears) > 0.01) {
            return $this->response->setJSON([
                'success' => false,
                'error'   => 'Amount Paid - Standard must match Standard Arrears (UGX)'
            ]);
        }

        if (abs($amountPaidInterest - $interest) > 0.01) {
            return $this->response->setJSON([
                'success' => false,
                'error'   => 'Amount Paid - Interest must match Interest (UGX)'
            ]);
        }

        // Check session data
        if (!session()->get('sat_parsed_data')) {
            return $this->response->setJSON([
                'success' => false,
                'error'   => 'SAT file data not found. Please upload your SAT file again.'
            ]);
        }

        if (!session()->get('payment_proof_path')) {
            return $this->response->setJSON([
                'success' => false,
                'error'   => 'Payment proof not found. Please upload your payment proof again.'
            ]);
        }

        // Start transaction
        $db = \Config\Database::connect();
        $db->transStart();

        try {
            // Generate reference number
            $reference = $this->applicationModel->generateReference();

            // Move files to permanent storage
            $satTempPath = session()->get('sat_file_path');
            $paymentTempPath = session()->get('payment_proof_path');

            $satFileName = session()->get('sat_file_name');
            $paymentFileName = session()->get('payment_proof_name');

            // Create upload directories if they don't exist
            $satDir = WRITEPATH . 'uploads/sat/';
            $paymentDir = WRITEPATH . 'uploads/proofs/';

            if (!is_dir($satDir)) {
                mkdir($satDir, 0755, true);
            }
            if (!is_dir($paymentDir)) {
                mkdir($paymentDir, 0755, true);
            }

            // Move files
            $satFinalPath = $satDir . $satFileName;
            $paymentFinalPath = $paymentDir . $paymentFileName;

            rename($satTempPath, $satFinalPath);
            rename($paymentTempPath, $paymentFinalPath);

            // Prepare application data
            $parsedData = session()->get('sat_parsed_data');
            $summary = $parsedData['summary'] ?? [];

            $applicationData = [
                'reference'            => $reference,
                'employer_number'      => $data['employer_number'],
                'employer_name'        => $data['employer_name'],
                'email'                => $data['email'],
                'phone'                => $data['phone'],
                'sector'               => $data['sector'],
                'standard_arrears'     => $this->parseNumber($data['standard_arrears']),
                'interest'             => $this->parseNumber($data['interest']),
                'penalty'              => $this->parseNumber($data['penalty'] ?? 0),
                'amount_paid_standard' => $this->parseNumber($data['amount_paid_standard']),
                'amount_paid_interest' => $this->parseNumber($data['amount_paid_interest']),
                'period_from'          => $data['period_from'] ?? $summary['start_date'],
                'period_to'            => $data['period_to'] ?? $summary['end_date'],
                'sat_file_path'        => $satFinalPath,
                'payment_reference'    => $data['payment_reference'],
                'payment_proof_path'   => $paymentFinalPath,
                'terms_accepted'       => 1,
                'terms_accepted_at'    => session()->get('terms_accepted_at'),
                'status'               => 'submitted',
            ];

            // Insert application
            $applicationId = $this->applicationModel->insert($applicationData);

            if (!$applicationId) {
                throw new \Exception('Failed to save application');
            }

            // Insert SAT summary data
            $summaryData = array_merge($summary, ['application_id' => $applicationId]);
            $this->satSummaryModel->insert($summaryData);

            // Insert monthly data
            $monthlyData = $parsedData['monthly'] ?? [];
            foreach ($monthlyData as &$row) {
                $row['application_id'] = $applicationId;
            }
            if (!empty($monthlyData)) {
                $this->satMonthlyModel->insertBatchRecords($monthlyData);
            }

            // Insert arrears data
            $arrearsData = $parsedData['arrears'] ?? [];
            foreach ($arrearsData as &$row) {
                $row['application_id'] = $applicationId;
            }
            if (!empty($arrearsData)) {
                $this->satArrearsModel->insertBatchRecords($arrearsData);
            }

            // Create sync log entries
            $this->syncLogModel->createSyncEntries($applicationId);

            $db->transComplete();

            if ($db->transStatus() === false) {
                throw new \Exception('Transaction failed');
            }

            // Clear session data
            session()->remove(['sat_file_path', 'sat_file_name', 'sat_parsed_data', 'payment_proof_path', 'payment_proof_name']);

            return $this->response->setJSON([
                'success'   => true,
                'reference' => $reference,
                'redirect'  => base_url("apply/success/{$reference}")
            ]);

        } catch (\Exception $e) {
            $db->transRollback();

            log_message('error', 'Application submission failed: ' . $e->getMessage());

            return $this->response->setJSON([
                'success' => false,
                'error'   => 'Failed to submit application. Please try again.'
            ]);
        }
    }

    /**
     * Success page
     */
    public function success(string $reference): string
    {
        return view('application/success', [
            'title'     => 'Application Submitted - NSSF Amnesty',
            'reference' => $reference
        ]);
    }

    /**
     * Check application status
     */
    public function checkStatus(string $reference): string
    {
        $application = $this->applicationModel->findByReference($reference);

        if (!$application) {
            return view('application/status', [
                'title'     => 'Check Status - NSSF Amnesty',
                'found'     => false,
                'reference' => $reference
            ]);
        }

        return view('application/status', [
            'title'       => 'Application Status - NSSF Amnesty',
            'found'       => true,
            'reference'   => $reference,
            'application' => $application
        ]);
    }

    /**
     * Parse number from formatted string
     */
    protected function parseNumber($value): float
    {
        if (is_numeric($value)) {
            return (float) $value;
        }
        // Remove commas and other formatting
        return (float) preg_replace('/[^0-9.-]/', '', $value);
    }
}
