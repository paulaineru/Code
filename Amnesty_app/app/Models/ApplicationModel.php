<?php

namespace App\Models;

use CodeIgniter\Model;

class ApplicationModel extends Model
{
    protected $table = 'amnesty_applications';
    protected $primaryKey = 'id';
    protected $useAutoIncrement = true;
    protected $returnType = 'array';
    protected $useSoftDeletes = false;
    protected $allowedFields = [
        'reference',
        'employer_number',
        'employer_name',
        'email',
        'phone',
        'sector',
        'standard_arrears',
        'interest',
        'penalty',
        'amount_paid_standard',
        'amount_paid_interest',
        'period_from',
        'period_to',
        'sat_file_path',
        'payment_reference',
        'payment_proof_path',
        'terms_accepted',
        'terms_accepted_at',
        'status',
        'rejection_reason',
    ];

    protected $useTimestamps = true;
    protected $createdField = 'created_at';
    protected $updatedField = 'updated_at';

    protected $validationRules = [
        'employer_number' => 'required|max_length[50]',
        'employer_name'   => 'required|max_length[255]',
        'email'           => 'required|valid_email|max_length[255]',
        'phone'           => 'required|max_length[20]',
        'sector'          => 'required|max_length[100]',
    ];

    protected $validationMessages = [
        'employer_number' => [
            'required' => 'Employer number is required',
        ],
        'employer_name' => [
            'required' => 'Employer name is required',
        ],
        'email' => [
            'required'    => 'Email address is required',
            'valid_email' => 'Please enter a valid email address',
        ],
        'phone' => [
            'required' => 'Phone number is required',
        ],
        'sector' => [
            'required' => 'Please select a sector',
        ],
    ];

    /**
     * Generate unique application reference number
     */
    public function generateReference(): string
    {
        $prefix = 'AMN';
        $year = date('Y');
        $random = strtoupper(substr(md5(uniqid()), 0, 6));
        return "{$prefix}-{$year}-{$random}";
    }

    /**
     * Find application by reference number
     */
    public function findByReference(string $reference): ?array
    {
        return $this->where('reference', $reference)->first();
    }

    /**
     * Get applications with optional filters
     */
    public function getApplications(array $filters = [], int $limit = 20, int $offset = 0): array
    {
        $builder = $this->builder();

        if (!empty($filters['status'])) {
            $builder->where('status', $filters['status']);
        }

        if (!empty($filters['employer_number'])) {
            $builder->like('employer_number', $filters['employer_number']);
        }

        if (!empty($filters['from_date'])) {
            $builder->where('created_at >=', $filters['from_date']);
        }

        if (!empty($filters['to_date'])) {
            $builder->where('created_at <=', $filters['to_date']);
        }

        return [
            'data'  => $builder->orderBy('created_at', 'DESC')->get($limit, $offset)->getResultArray(),
            'total' => $this->countAllResults(false),
        ];
    }
}
