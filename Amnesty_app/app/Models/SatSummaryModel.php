<?php

namespace App\Models;

use CodeIgniter\Model;

class SatSummaryModel extends Model
{
    protected $table = 'sat_summary';
    protected $primaryKey = 'id';
    protected $useAutoIncrement = true;
    protected $returnType = 'array';
    protected $useSoftDeletes = false;
    protected $allowedFields = [
        'application_id',
        'employer_name',
        'employer_number',
        'start_date',
        'end_date',
        'financial_year',
        'num_employees',
        'total_basic_wages',
        'non_resident_salaries',
        'gross_earnings',
        'contributions_due',
        'special_contribution_due',
        'special_contribution_paid',
        'contributions_paid',
        'contribution_arrears',
        'special_contribution_arrears',
        'penalty_on_arrears',
        'interest_rate',
        'interest_on_arrears',
        'arrears_discovered_15',
        'special_contribution_10',
        'total_interest',
        'outstanding_arrears_interest',
        'total_penalty',
        'total_amount',
    ];

    protected $useTimestamps = true;
    protected $createdField = 'created_at';
    protected $updatedField = 'updated_at';

    /**
     * Get summary by application ID
     */
    public function getByApplicationId(int $applicationId): ?array
    {
        return $this->where('application_id', $applicationId)->first();
    }
}
