<?php

namespace App\Models;

use CodeIgniter\Model;

class SyncLogModel extends Model
{
    protected $table = 'sync_log';
    protected $primaryKey = 'id';
    protected $useAutoIncrement = true;
    protected $returnType = 'array';
    protected $useSoftDeletes = false;
    protected $allowedFields = [
        'application_id',
        'sync_type',
        'status',
        'remote_id',
        'attempts',
        'last_attempt_at',
        'error_message',
    ];

    protected $useTimestamps = true;
    protected $createdField = 'created_at';
    protected $updatedField = 'updated_at';

    // Sync status constants
    const STATUS_PENDING = 'pending';
    const STATUS_IN_PROGRESS = 'in_progress';
    const STATUS_SYNCED = 'synced';
    const STATUS_FAILED = 'failed';

    // Sync type constants
    const TYPE_API = 'api';
    const TYPE_FILE_SAT = 'file_sat';
    const TYPE_FILE_PAYMENT = 'file_payment';

    /**
     * Get pending syncs for retry
     */
    public function getPendingSyncs(int $limit = 50): array
    {
        return $this->whereIn('status', [self::STATUS_PENDING, self::STATUS_FAILED])
                    ->where('attempts <', 5)
                    ->orderBy('created_at', 'ASC')
                    ->findAll($limit);
    }

    /**
     * Get sync logs by application ID
     */
    public function getByApplicationId(int $applicationId): array
    {
        return $this->where('application_id', $applicationId)
                    ->orderBy('created_at', 'DESC')
                    ->findAll();
    }

    /**
     * Mark sync as in progress
     */
    public function markInProgress(int $id): bool
    {
        return $this->update($id, [
            'status'          => self::STATUS_IN_PROGRESS,
            'last_attempt_at' => date('Y-m-d H:i:s'),
        ]);
    }

    /**
     * Mark sync as successful
     */
    public function markSynced(int $id, ?string $remoteId = null): bool
    {
        $data = [
            'status'          => self::STATUS_SYNCED,
            'last_attempt_at' => date('Y-m-d H:i:s'),
        ];
        if ($remoteId) {
            $data['remote_id'] = $remoteId;
        }
        return $this->update($id, $data);
    }

    /**
     * Mark sync as failed
     */
    public function markFailed(int $id, string $errorMessage): bool
    {
        $current = $this->find($id);
        return $this->update($id, [
            'status'          => self::STATUS_FAILED,
            'attempts'        => ($current['attempts'] ?? 0) + 1,
            'last_attempt_at' => date('Y-m-d H:i:s'),
            'error_message'   => $errorMessage,
        ]);
    }

    /**
     * Create sync entries for a new application
     */
    public function createSyncEntries(int $applicationId): bool
    {
        $types = [self::TYPE_API, self::TYPE_FILE_SAT, self::TYPE_FILE_PAYMENT];
        $now = date('Y-m-d H:i:s');

        foreach ($types as $type) {
            $this->insert([
                'application_id' => $applicationId,
                'sync_type'      => $type,
                'status'         => self::STATUS_PENDING,
                'attempts'       => 0,
                'created_at'     => $now,
                'updated_at'     => $now,
            ]);
        }

        return true;
    }
}
