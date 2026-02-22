<?php
/**
 * SAT Sample File Generator
 *
 * Generates sample Excel files for testing the NSSF Amnesty application.
 * Run from project root: php public/assets/sat-template/generate-sample.php
 */

require_once __DIR__ . '/../../../vendor/autoload.php';

use PhpOffice\PhpSpreadsheet\Spreadsheet;
use PhpOffice\PhpSpreadsheet\Writer\Xlsx;
use PhpOffice\PhpSpreadsheet\Style\NumberFormat;

class SampleFileGenerator
{
    private string $outputDir;

    public function __construct()
    {
        $this->outputDir = __DIR__;
    }

    /**
     * Generate realistic test file
     */
    public function generateRealistic(): string
    {
        $spreadsheet = new Spreadsheet();

        // Create Summary sheet
        $summarySheet = $spreadsheet->getActiveSheet();
        $summarySheet->setTitle('Summary');
        $this->populateSummarySheet($summarySheet, $this->getRealisticSummaryData());

        // Create Monthly Totals sheet
        $monthlySheet = $spreadsheet->createSheet();
        $monthlySheet->setTitle('Monthly Totals');
        $this->populateMonthlySheet($monthlySheet, $this->getRealisticMonthlyData());

        // Create Arrears-Employee Details sheet
        $arrearsSheet = $spreadsheet->createSheet();
        $arrearsSheet->setTitle('Arrears-Employee Details');
        $this->populateArrearsSheet($arrearsSheet, $this->getRealisticArrearsData());

        // Save file
        $filename = $this->outputDir . '/sample-test-realistic.xlsx';
        $writer = new Xlsx($spreadsheet);
        $writer->save($filename);

        return $filename;
    }

    /**
     * Generate edge case test file
     */
    public function generateEdgeCases(): string
    {
        $spreadsheet = new Spreadsheet();

        // Create Summary sheet
        $summarySheet = $spreadsheet->getActiveSheet();
        $summarySheet->setTitle('Summary');
        $this->populateSummarySheet($summarySheet, $this->getEdgeCaseSummaryData());

        // Create Monthly Totals sheet
        $monthlySheet = $spreadsheet->createSheet();
        $monthlySheet->setTitle('Monthly Totals');
        $this->populateMonthlySheet($monthlySheet, $this->getEdgeCaseMonthlyData());

        // Create Arrears-Employee Details sheet
        $arrearsSheet = $spreadsheet->createSheet();
        $arrearsSheet->setTitle('Arrears-Employee Details');
        $this->populateArrearsSheet($arrearsSheet, $this->getEdgeCaseArrearsData());

        // Save file
        $filename = $this->outputDir . '/sample-test-edge-cases.xlsx';
        $writer = new Xlsx($spreadsheet);
        $writer->save($filename);

        return $filename;
    }

    /**
     * Populate Summary sheet with labels and data
     */
    private function populateSummarySheet($sheet, array $data): void
    {
        // Add labels in column B for readability
        $labels = [
            5 => 'Employer Name',
            6 => 'Employer Number',
            7 => 'Start Date',
            8 => 'End Date',
            9 => 'Financial Year',
            10 => 'Number of Employees',
            12 => 'Total Basic Wages',
            13 => 'Non-Resident Salaries',
            14 => 'Gross Earnings',
            16 => 'Contributions Due',
            17 => 'Special Contribution Due',
            19 => 'Contributions Paid',
            20 => 'Special Contribution Paid',
            22 => 'Contribution Arrears',
            23 => 'Special Contribution Arrears',
            25 => 'Penalty on Arrears',
            26 => 'Interest Rate',
            27 => 'Interest on Arrears',
            30 => 'Arrears Discovered (15%)',
            31 => 'Special Contribution (10%)',
            33 => 'Total Interest',
            34 => 'Outstanding Arrears + Interest',
            35 => 'Total Penalty',
            36 => 'Total Amount',
        ];

        foreach ($labels as $row => $label) {
            $sheet->setCellValue("B{$row}", $label);
        }

        // Set data values in column C
        $cellMapping = [
            'C5' => $data['employer_name'],
            'C6' => $data['employer_number'],
            'C7' => $data['start_date'],
            'C8' => $data['end_date'],
            'C9' => $data['financial_year'],
            'C10' => $data['num_employees'],
            'C12' => $data['total_basic_wages'],
            'C13' => $data['non_resident_salaries'],
            'C14' => $data['gross_earnings'],
            'C16' => $data['contributions_due'],
            'C17' => $data['special_contribution_due'],
            'C19' => $data['contributions_paid'],
            'C20' => $data['special_contribution_paid'],
            'C22' => $data['contribution_arrears'],
            'C23' => $data['special_contribution_arrears'],
            'C25' => $data['penalty_on_arrears'],
            'C26' => $data['interest_rate'],
            'C27' => $data['interest_on_arrears'],
            'C30' => $data['arrears_discovered_15'],
            'C31' => $data['special_contribution_10'],
            'C33' => $data['total_interest'],
            'C34' => $data['outstanding_arrears_interest'],
            'C35' => $data['total_penalty'],
            'C36' => $data['total_amount'],
        ];

        foreach ($cellMapping as $cell => $value) {
            $sheet->setCellValue($cell, $value);
        }
    }

    /**
     * Populate Monthly Totals sheet with headers and data
     */
    private function populateMonthlySheet($sheet, array $rows): void
    {
        // Add headers in rows 1-2
        $headers = [
            'A' => 'Month/Year',
            'B' => 'No. of Employees',
            'C' => 'Salaries & Wages',
            'D' => 'Non-Resident Salaries',
            'E' => 'Additional Cash Payments',
            'F' => 'Gross Earnings',
            'G' => 'Standard Contribution',
            'H' => 'Special Contribution',
            'I' => 'Contributions Deductible',
            'J' => 'Standard Remitted',
            'K' => 'Special Remitted',
            'L' => 'Due Date',
            'M' => 'Arrears (15%)',
            'N' => 'Arrears (10%)',
            'O' => 'Total Arrears',
            'P' => 'Months in Arrears',
            'Q' => 'Penalty',
        ];

        foreach ($headers as $col => $header) {
            $sheet->setCellValue("{$col}1", $header);
        }

        // Add data starting from row 3
        $rowNum = 3;
        foreach ($rows as $row) {
            $sheet->setCellValue("A{$rowNum}", $row['month_year']);
            $sheet->setCellValue("B{$rowNum}", $row['num_employees']);
            $sheet->setCellValue("C{$rowNum}", $row['salaries_wages']);
            $sheet->setCellValue("D{$rowNum}", $row['non_resident_salaries']);
            $sheet->setCellValue("E{$rowNum}", $row['additional_cash_payments']);
            $sheet->setCellValue("F{$rowNum}", $row['gross_earnings']);
            $sheet->setCellValue("G{$rowNum}", $row['standard_contribution']);
            $sheet->setCellValue("H{$rowNum}", $row['special_contribution']);
            $sheet->setCellValue("I{$rowNum}", $row['contributions_deductable']);
            $sheet->setCellValue("J{$rowNum}", $row['standard_remitted']);
            $sheet->setCellValue("K{$rowNum}", $row['special_remitted']);
            $sheet->setCellValue("L{$rowNum}", $row['due_date']);
            $sheet->setCellValue("M{$rowNum}", $row['arrears_15']);
            $sheet->setCellValue("N{$rowNum}", $row['arrears_10']);
            $sheet->setCellValue("O{$rowNum}", $row['total_arrears']);
            $sheet->setCellValue("P{$rowNum}", $row['months_in_arrears']);
            $sheet->setCellValue("Q{$rowNum}", $row['penalty']);
            $rowNum++;
        }
    }

    /**
     * Populate Arrears-Employee Details sheet with headers and data
     */
    private function populateArrearsSheet($sheet, array $rows): void
    {
        // Add headers in rows 1-2
        $headers = [
            'A' => 'NSSF Number',
            'B' => 'Contribution Type',
            'C' => 'Contribution Year',
            'D' => 'Contribution Month',
            'E' => 'Employee Name',
            'F' => 'Employee Gross Pay',
            'G' => 'NSSF Contribution',
            'H' => 'Remitted',
            'I' => 'Arrears',
            'J' => 'Narration',
        ];

        foreach ($headers as $col => $header) {
            $sheet->setCellValue("{$col}1", $header);
        }

        // Add data starting from row 3
        $rowNum = 3;
        foreach ($rows as $row) {
            $sheet->setCellValue("A{$rowNum}", $row['nssf_number']);
            $sheet->setCellValue("B{$rowNum}", $row['contribution_type']);
            $sheet->setCellValue("C{$rowNum}", $row['contribution_year']);
            $sheet->setCellValue("D{$rowNum}", $row['contribution_month']);
            $sheet->setCellValue("E{$rowNum}", $row['employee_name']);
            $sheet->setCellValue("F{$rowNum}", $row['employee_gross_pay']);
            $sheet->setCellValue("G{$rowNum}", $row['nssf_contribution']);
            $sheet->setCellValue("H{$rowNum}", $row['remitted']);
            $sheet->setCellValue("I{$rowNum}", $row['arrears']);
            $sheet->setCellValue("J{$rowNum}", $row['narration']);
            $rowNum++;
        }
    }

    // ==================== REALISTIC DATA ====================

    private function getRealisticSummaryData(): array
    {
        return [
            'employer_name' => 'Acme Manufacturing Ltd',
            'employer_number' => 'EMP2024001',
            'start_date' => '2024-01-01',
            'end_date' => '2024-12-31',
            'financial_year' => 'FY 2024-25',
            'num_employees' => 18,
            'total_basic_wages' => 324000000,
            'non_resident_salaries' => 36000000,
            'gross_earnings' => 360000000,
            'contributions_due' => 18000000,
            'special_contribution_due' => 3600000,
            'contributions_paid' => 14400000,
            'special_contribution_paid' => 2880000,
            'contribution_arrears' => 3600000,
            'special_contribution_arrears' => 720000,
            'penalty_on_arrears' => 540000,
            'interest_rate' => 0.15,
            'interest_on_arrears' => 648000,
            'arrears_discovered_15' => 540000,
            'special_contribution_10' => 72000,
            'total_interest' => 612000,
            'outstanding_arrears_interest' => 4932000,
            'total_penalty' => 540000,
            'total_amount' => 5472000,
        ];
    }

    private function getRealisticMonthlyData(): array
    {
        $months = [];
        $baseEmployees = 15;
        $baseSalary = 1500000; // UGX per employee

        for ($m = 1; $m <= 12; $m++) {
            $numEmployees = $baseEmployees + rand(-2, 3); // Vary employee count
            $salaries = $numEmployees * $baseSalary;
            $nonResident = round($salaries * 0.1);
            $additional = round($salaries * 0.05);
            $gross = $salaries + $nonResident + $additional;
            $standardContrib = round($gross * 0.05);
            $specialContrib = round($gross * 0.01);

            // Simulate some months with arrears
            $hasArrears = in_array($m, [3, 6, 9, 11]);
            $standardRemitted = $hasArrears ? round($standardContrib * 0.7) : $standardContrib;
            $specialRemitted = $hasArrears ? round($specialContrib * 0.7) : $specialContrib;

            $arrears15 = $hasArrears ? $standardContrib - $standardRemitted : 0;
            $arrears10 = $hasArrears ? $specialContrib - $specialRemitted : 0;
            $totalArrears = $arrears15 + $arrears10;
            $monthsInArrears = $hasArrears ? rand(1, 6) : 0;
            $penalty = $hasArrears ? round($totalArrears * 0.15 * $monthsInArrears / 12) : 0;

            $months[] = [
                'month_year' => "2024-{$m}-01",
                'num_employees' => $numEmployees,
                'salaries_wages' => $salaries,
                'non_resident_salaries' => $nonResident,
                'additional_cash_payments' => $additional,
                'gross_earnings' => $gross,
                'standard_contribution' => $standardContrib,
                'special_contribution' => $specialContrib,
                'contributions_deductable' => $standardContrib + $specialContrib,
                'standard_remitted' => $standardRemitted,
                'special_remitted' => $specialRemitted,
                'due_date' => "2024-{$m}-15",
                'arrears_15' => $arrears15,
                'arrears_10' => $arrears10,
                'total_arrears' => $totalArrears,
                'months_in_arrears' => $monthsInArrears,
                'penalty' => $penalty,
            ];
        }

        return $months;
    }

    private function getRealisticArrearsData(): array
    {
        $employees = [
            ['name' => 'John Mukasa', 'nssf' => 'NSSF001234567', 'gross' => 2500000],
            ['name' => 'Sarah Namubiru', 'nssf' => 'NSSF001234568', 'gross' => 1800000],
            ['name' => 'Peter Okello', 'nssf' => 'NSSF001234569', 'gross' => 2200000],
            ['name' => 'Grace Namutebi', 'nssf' => 'NSSF001234570', 'gross' => 1500000],
            ['name' => 'David Ssempala', 'nssf' => 'NSSF001234571', 'gross' => 3000000],
            ['name' => 'Mary Achieng', 'nssf' => 'NSSF001234572', 'gross' => 1200000],
        ];

        $arrears = [];
        $arrearsMonths = [3, 6, 9, 11];
        $types = ['Standard', 'Special'];

        foreach ($employees as $emp) {
            foreach ($arrearsMonths as $month) {
                foreach ($types as $type) {
                    $rate = $type === 'Standard' ? 0.05 : 0.01;
                    $contribution = round($emp['gross'] * $rate);
                    $remitted = round($contribution * 0.7);

                    $arrears[] = [
                        'nssf_number' => $emp['nssf'],
                        'contribution_type' => $type,
                        'contribution_year' => 2024,
                        'contribution_month' => $month,
                        'employee_name' => $emp['name'],
                        'employee_gross_pay' => $emp['gross'],
                        'nssf_contribution' => $contribution,
                        'remitted' => $remitted,
                        'arrears' => $contribution - $remitted,
                        'narration' => 'Partial payment - cash flow issues',
                    ];
                }
            }
        }

        return $arrears;
    }

    // ==================== EDGE CASE DATA ====================

    private function getEdgeCaseSummaryData(): array
    {
        return [
            'employer_name' => "O'Brien & Associates (K) Ltd",  // Special characters
            'employer_number' => 'EMP999999999',
            'start_date' => '2024-12-01',  // Single month period
            'end_date' => '2024-12-31',
            'financial_year' => 'FY 2024-25',
            'num_employees' => 1,  // Minimum employees
            'total_basic_wages' => 9999999999.99,  // Large number with decimals
            'non_resident_salaries' => 0,  // Zero value
            'gross_earnings' => 9999999999.99,
            'contributions_due' => 499999999.9995,  // Many decimal places
            'special_contribution_due' => 99999999.9999,
            'contributions_paid' => 0,  // Zero - nothing paid
            'special_contribution_paid' => 0,
            'contribution_arrears' => 499999999.9995,
            'special_contribution_arrears' => 99999999.9999,
            'penalty_on_arrears' => 0,  // Zero penalty
            'interest_rate' => 0.0001,  // Very small rate
            'interest_on_arrears' => 59999.9999,
            'arrears_discovered_15' => 74999999.999925,
            'special_contribution_10' => 9999999.9999,
            'total_interest' => 84999999.999825,
            'outstanding_arrears_interest' => 684999499.99905,
            'total_penalty' => 0,
            'total_amount' => 684999499.99905,
        ];
    }

    private function getEdgeCaseMonthlyData(): array
    {
        return [
            // Single month with zero values
            [
                'month_year' => '2024-12-01',
                'num_employees' => 1,
                'salaries_wages' => 9999999999.99,
                'non_resident_salaries' => 0,
                'additional_cash_payments' => 0,
                'gross_earnings' => 9999999999.99,
                'standard_contribution' => 499999999.9995,
                'special_contribution' => 99999999.9999,
                'contributions_deductable' => 599999499.9994,
                'standard_remitted' => 0,
                'special_remitted' => 0,
                'due_date' => '2024-12-15',
                'arrears_15' => 499999999.9995,
                'arrears_10' => 99999999.9999,
                'total_arrears' => 599999499.9994,
                'months_in_arrears' => 1,
                'penalty' => 0,
            ],
            // Month at beginning of year - date boundary
            [
                'month_year' => '2024-01-01',
                'num_employees' => 0,  // Zero employees edge case
                'salaries_wages' => 0,
                'non_resident_salaries' => 0,
                'additional_cash_payments' => 0,
                'gross_earnings' => 0,
                'standard_contribution' => 0,
                'special_contribution' => 0,
                'contributions_deductable' => 0,
                'standard_remitted' => 0,
                'special_remitted' => 0,
                'due_date' => '2024-01-15',
                'arrears_15' => 0,
                'arrears_10' => 0,
                'total_arrears' => 0,
                'months_in_arrears' => 0,
                'penalty' => 0,
            ],
        ];
    }

    private function getEdgeCaseArrearsData(): array
    {
        return [
            // Special characters in name
            [
                'nssf_number' => 'NSSF000000001',
                'contribution_type' => 'Standard',
                'contribution_year' => 2024,
                'contribution_month' => 12,
                'employee_name' => "Jean-Pierre O'Connor-Smith",
                'employee_gross_pay' => 9999999999.99,
                'nssf_contribution' => 499999999.9995,
                'remitted' => 0,
                'arrears' => 499999999.9995,
                'narration' => 'Test with special chars: <>&"\'',
            ],
            // Empty/null-like values
            [
                'nssf_number' => 'NSSF000000002',
                'contribution_type' => 'Special',
                'contribution_year' => 2024,
                'contribution_month' => 12,
                'employee_name' => 'X',  // Minimum name length
                'employee_gross_pay' => 0.01,  // Minimum positive value
                'nssf_contribution' => 0.0005,
                'remitted' => 0,
                'arrears' => 0.0005,
                'narration' => '',  // Empty narration
            ],
            // Maximum values
            [
                'nssf_number' => 'NSSF999999999',
                'contribution_type' => 'Standard',
                'contribution_year' => 2024,
                'contribution_month' => 1,
                'employee_name' => 'ABCDEFGHIJKLMNOPQRSTUVWXYZ ABCDEFGHIJKLMNOPQRSTUVWXYZ',  // Long name
                'employee_gross_pay' => 99999999999.99,
                'nssf_contribution' => 4999999999.9995,
                'remitted' => 4999999999.9995,
                'arrears' => 0,  // Zero arrears despite high contribution
                'narration' => 'Fully paid - no arrears outstanding for this period',
            ],
        ];
    }
}

// Run generator
echo "SAT Sample File Generator\n";
echo "=========================\n\n";

$generator = new SampleFileGenerator();

echo "Generating realistic test file...\n";
$realisticFile = $generator->generateRealistic();
echo "Created: {$realisticFile}\n\n";

echo "Generating edge case test file...\n";
$edgeCaseFile = $generator->generateEdgeCases();
echo "Created: {$edgeCaseFile}\n\n";

echo "Done! You can now upload these files to test the application.\n";
