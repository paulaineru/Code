<?php

namespace App\Models;

use CodeIgniter\Model;

class SatArrearsModel extends Model
{
    protected $table = 'sat_arrears_employees';
    protected $primaryKey = 'id';
    protected $useAutoIncrement = true;
    protected $returnType = 'array';
    protected $useSoftDeletes = false;
    protected $allowedFields = [
        'application_id',
        'nssf_number',
        'contribution_type',
        'contribution_year',
        'contribution_month',
        'employee_name',
        'employee_gross_pay',
        'nssf_contribution',
        'remitted',
        'arrears',
        'narration',
    ];

    protected $useTimestamps = true;
    protected $createdField = 'created_at';
    protected $updatedField = 'updated_at';

    /**
     * Get all employee arrears by application ID
     */
    public function getByApplicationId(int $applicationId): array
    {
        return $this->where('application_id', $applicationId)
                    ->orderBy('contribution_year', 'ASC')
                    ->orderBy('contribution_month', 'ASC')
                    ->findAll();
    }

    /**
     * Insert multiple arrears records
     */
    public function insertBatchRecords(array $data): bool
    {
        if (empty($data)) {
            return true;
        }
        return $this->db->table($this->table)->insertBatch($data);
    }

    /**
     * Get arrears summary statistics
     */
    public function getArrearsSummary(int $applicationId): array
    {
        $builder = $this->builder();
        $builder->select('COUNT(*) as total_records, SUM(arrears) as total_arrears');
        $builder->where('application_id', $applicationId);
        return $builder->get()->getRowArray();
    }
}
