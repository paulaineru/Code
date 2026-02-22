<?php

namespace App\Models;

use CodeIgniter\Model;

class SatMonthlyModel extends Model
{
    protected $table = 'sat_monthly_totals';
    protected $primaryKey = 'id';
    protected $useAutoIncrement = true;
    protected $returnType = 'array';
    protected $useSoftDeletes = false;
    protected $allowedFields = [
        'application_id',
        'month_year',
        'num_employees',
        'salaries_wages',
        'non_resident_salaries',
        'additional_cash_payments',
        'gross_earnings',
        'standard_contribution',
        'special_contribution',
        'contributions_deductable',
        'standard_remitted',
        'special_remitted',
        'due_date',
        'arrears_15',
        'arrears_10',
        'total_arrears',
        'months_in_arrears',
        'penalty',
    ];

    protected $useTimestamps = true;
    protected $createdField = 'created_at';
    protected $updatedField = 'updated_at';

    /**
     * Get all monthly records by application ID
     */
    public function getByApplicationId(int $applicationId): array
    {
        return $this->where('application_id', $applicationId)
                    ->orderBy('month_year', 'ASC')
                    ->findAll();
    }

    /**
     * Insert multiple monthly records
     */
    public function insertBatchRecords(array $data): bool
    {
        if (empty($data)) {
            return true;
        }
        return $this->db->table($this->table)->insertBatch($data);
    }
}
