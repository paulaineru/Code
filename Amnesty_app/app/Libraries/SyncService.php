<?php

namespace App\Libraries;

use App\Models\ApplicationModel;
use App\Models\SatSummaryModel;
use App\Models\SatMonthlyModel;
use App\Models\SatArrearsModel;
use App\Models\SyncLogModel;

class SyncService
{
    protected ApplicationModel $applicationModel;
    protected SatSummaryModel $satSummaryModel;
    protected SatMonthlyModel $satMonthlyModel;
    protected SatArrearsModel $satArrearsModel;
    protected SyncLogModel $syncLogModel;

    protected string $remoteUrl;
    protected string $remoteApiKey;

    public function __construct()
    {
        $this->applicationModel = new ApplicationModel();
        $this->satSummaryModel = new SatSummaryModel();
        $this->satMonthlyModel = new SatMonthlyModel();
        $this->satArrearsModel = new SatArrearsModel();
        $this->syncLogModel = new SyncLogModel();

        $this->remoteUrl = getenv('REMOTE_SYNC_URL') ?: '';
        $this->remoteApiKey = getenv('REMOTE_SYNC_API_KEY') ?: '';
    }

    /**
     * Sync a single application
     */
    public function syncApplication(int $applicationId): array
    {
        $results = [
            'api'          => false,
            'file_sat'     => false,
            'file_payment' => false,
            'errors'       => [],
        ];

        if (empty($this->remoteUrl)) {
            $results['errors'][] = 'Remote sync URL not configured';
            return $results;
        }

        $application = $this->applicationModel->find($applicationId);
        if (!$application) {
            $results['errors'][] = 'Application not found';
            return $results;
        }

        // Sync API data
        $apiResult = $this->syncApiData($applicationId, $application);
        $results['api'] = $apiResult['success'];
        if (!$apiResult['success']) {
            $results['errors'][] = 'API sync: ' . ($apiResult['error'] ?? 'Unknown error');
        }

        // Sync SAT file
        $satResult = $this->syncFile($applicationId, $application['sat_file_path'], 'file_sat');
        $results['file_sat'] = $satResult['success'];
        if (!$satResult['success']) {
            $results['errors'][] = 'SAT file sync: ' . ($satResult['error'] ?? 'Unknown error');
        }

        // Sync payment proof
        $paymentResult = $this->syncFile($applicationId, $application['payment_proof_path'], 'file_payment');
        $results['file_payment'] = $paymentResult['success'];
        if (!$paymentResult['success']) {
            $results['errors'][] = 'Payment proof sync: ' . ($paymentResult['error'] ?? 'Unknown error');
        }

        return $results;
    }

    /**
     * Sync application data via API
     */
    protected function syncApiData(int $applicationId, array $application): array
    {
        $syncLog = $this->getSyncLog($applicationId, SyncLogModel::TYPE_API);

        if (!$syncLog) {
            return ['success' => false, 'error' => 'Sync log not found'];
        }

        $this->syncLogModel->markInProgress($syncLog['id']);

        try {
            // Prepare data payload
            $payload = [
                'application' => $application,
                'sat_summary' => $this->satSummaryModel->getByApplicationId($applicationId),
                'sat_monthly' => $this->satMonthlyModel->getByApplicationId($applicationId),
                'sat_arrears' => $this->satArrearsModel->getByApplicationId($applicationId),
            ];

            // Send to remote server
            $response = $this->sendRequest(
                $this->remoteUrl . '/amnestyapi/receive_application',
                'POST',
                $payload
            );

            if ($response['success']) {
                $remoteId = $response['data']['id'] ?? null;
                $this->syncLogModel->markSynced($syncLog['id'], $remoteId);
                return ['success' => true, 'remote_id' => $remoteId];
            } else {
                $this->syncLogModel->markFailed($syncLog['id'], $response['error']);
                return ['success' => false, 'error' => $response['error']];
            }

        } catch (\Exception $e) {
            $this->syncLogModel->markFailed($syncLog['id'], $e->getMessage());
            return ['success' => false, 'error' => $e->getMessage()];
        }
    }

    /**
     * Sync a file to remote server
     */
    protected function syncFile(int $applicationId, ?string $filePath, string $syncType): array
    {
        $syncLog = $this->getSyncLog($applicationId, $syncType);

        if (!$syncLog) {
            return ['success' => false, 'error' => 'Sync log not found'];
        }

        if (!$filePath || !file_exists($filePath)) {
            $this->syncLogModel->markFailed($syncLog['id'], 'File not found');
            return ['success' => false, 'error' => 'File not found'];
        }

        // Get application for reference
        $application = $this->applicationModel->find($applicationId);
        if (!$application) {
            $this->syncLogModel->markFailed($syncLog['id'], 'Application not found');
            return ['success' => false, 'error' => 'Application not found'];
        }

        $this->syncLogModel->markInProgress($syncLog['id']);

        try {
            $response = $this->uploadFile(
                $this->remoteUrl . '/amnestyapi/receive_files',
                $filePath,
                [
                    'reference'  => $application['reference'],
                    'file_type'  => $syncType === SyncLogModel::TYPE_FILE_SAT ? 'sat_file' : 'payment_proof',
                ]
            );

            if ($response['success']) {
                $this->syncLogModel->markSynced($syncLog['id']);
                return ['success' => true];
            } else {
                $this->syncLogModel->markFailed($syncLog['id'], $response['error']);
                return ['success' => false, 'error' => $response['error']];
            }

        } catch (\Exception $e) {
            $this->syncLogModel->markFailed($syncLog['id'], $e->getMessage());
            return ['success' => false, 'error' => $e->getMessage()];
        }
    }

    /**
     * Get sync log entry
     */
    protected function getSyncLog(int $applicationId, string $syncType): ?array
    {
        $logs = $this->syncLogModel
            ->where('application_id', $applicationId)
            ->where('sync_type', $syncType)
            ->first();
        return $logs;
    }

    /**
     * Send HTTP request
     */
    protected function sendRequest(string $url, string $method, array $data): array
    {
        $ch = curl_init();

        $jsonData = json_encode($data);
        $timestamp = time();
        $signature = hash_hmac('sha256', $this->remoteApiKey . $timestamp . $jsonData, $this->remoteApiKey);

        curl_setopt_array($ch, [
            CURLOPT_URL            => $url,
            CURLOPT_RETURNTRANSFER => true,
            CURLOPT_TIMEOUT        => 60,
            CURLOPT_CUSTOMREQUEST  => $method,
            CURLOPT_POSTFIELDS     => $jsonData,
            CURLOPT_HTTPHEADER     => [
                'Content-Type: application/json',
                'X-API-Key: ' . $this->remoteApiKey,
                'X-Timestamp: ' . $timestamp,
                'X-Signature: ' . $signature,
            ],
        ]);

        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
        $error = curl_error($ch);

        curl_close($ch);

        if ($error) {
            return ['success' => false, 'error' => $error];
        }

        if ($httpCode >= 200 && $httpCode < 300) {
            return ['success' => true, 'data' => json_decode($response, true)];
        }

        return ['success' => false, 'error' => "HTTP $httpCode: $response"];
    }

    /**
     * Upload file to remote server
     */
    protected function uploadFile(string $url, string $filePath, array $extraData = []): array
    {
        $ch = curl_init();

        $timestamp = time();
        $signature = hash_hmac('sha256', $this->remoteApiKey . $timestamp, $this->remoteApiKey);

        $postData = $extraData;
        $postData['file'] = new \CURLFile($filePath);
        $postData['original_filename'] = basename($filePath);

        curl_setopt_array($ch, [
            CURLOPT_URL            => $url,
            CURLOPT_RETURNTRANSFER => true,
            CURLOPT_TIMEOUT        => 120,
            CURLOPT_POST           => true,
            CURLOPT_POSTFIELDS     => $postData,
            CURLOPT_HTTPHEADER     => [
                'X-API-Key: ' . $this->remoteApiKey,
                'X-Timestamp: ' . $timestamp,
                'X-Signature: ' . $signature,
            ],
        ]);

        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
        $error = curl_error($ch);

        curl_close($ch);

        if ($error) {
            return ['success' => false, 'error' => $error];
        }

        if ($httpCode >= 200 && $httpCode < 300) {
            return ['success' => true, 'data' => json_decode($response, true)];
        }

        return ['success' => false, 'error' => "HTTP $httpCode: $response"];
    }

    /**
     * Process pending syncs with retry logic
     */
    public function processPendingSyncs(int $limit = 50): array
    {
        $results = [
            'processed' => 0,
            'succeeded' => 0,
            'failed'    => 0,
        ];

        $pendingSyncs = $this->syncLogModel->getPendingSyncs($limit);

        foreach ($pendingSyncs as $sync) {
            $results['processed']++;

            // Exponential backoff check
            if ($sync['attempts'] > 0) {
                $backoffMinutes = pow(2, $sync['attempts']);
                $lastAttempt = strtotime($sync['last_attempt_at']);
                $nextRetry = $lastAttempt + ($backoffMinutes * 60);

                if (time() < $nextRetry) {
                    continue; // Skip, not ready for retry yet
                }
            }

            $application = $this->applicationModel->find($sync['application_id']);
            if (!$application) {
                $this->syncLogModel->markFailed($sync['id'], 'Application not found');
                $results['failed']++;
                continue;
            }

            $success = false;

            switch ($sync['sync_type']) {
                case SyncLogModel::TYPE_API:
                    $result = $this->syncApiData($sync['application_id'], $application);
                    $success = $result['success'];
                    break;

                case SyncLogModel::TYPE_FILE_SAT:
                    $result = $this->syncFile($sync['application_id'], $application['sat_file_path'], $sync['sync_type']);
                    $success = $result['success'];
                    break;

                case SyncLogModel::TYPE_FILE_PAYMENT:
                    $result = $this->syncFile($sync['application_id'], $application['payment_proof_path'], $sync['sync_type']);
                    $success = $result['success'];
                    break;
            }

            if ($success) {
                $results['succeeded']++;
            } else {
                $results['failed']++;
            }
        }

        return $results;
    }
}
