<?php

namespace App\Libraries;

use PhpOffice\PhpSpreadsheet\IOFactory;
use PhpOffice\PhpSpreadsheet\Spreadsheet;
use PhpOffice\PhpSpreadsheet\Worksheet\Worksheet;

class SatParser
{
    protected ?Spreadsheet $spreadsheet = null;
    protected array $errors = [];
    protected array $parsedData = [];

    // Expected sheet names
    const SHEET_SUMMARY = 'Summary';
    const SHEET_MONTHLY = 'Monthly Totals';
    const SHEET_ARREARS = 'Arrears-Employee Details';

    /**
     * Parse a SAT Excel file
     */
    public function parse(string $filePath): bool
    {
        $this->errors = [];
        $this->parsedData = [];

        // Validate file exists
        if (!file_exists($filePath)) {
            $this->errors[] = 'File not found';
            return false;
        }

        // Validate file extension
        $extension = strtolower(pathinfo($filePath, PATHINFO_EXTENSION));
        if (!in_array($extension, ['xlsm', 'xlsx', 'xls'])) {
            $this->errors[] = 'Invalid file type. Please upload an Excel file (.xlsm, .xlsx, .xls)';
            return false;
        }

        try {
            $this->spreadsheet = IOFactory::load($filePath);
        } catch (\Exception $e) {
            $this->errors[] = 'Unable to read Excel file: ' . $e->getMessage();
            return false;
        }

        // Validate required sheets exist
        if (!$this->validateSheets()) {
            return false;
        }

        // Parse each sheet
        $this->parseSummarySheet();
        $this->parseMonthlySheet();
        $this->parseArrearsSheet();

        return empty($this->errors);
    }

    /**
     * Validate required sheets exist
     */
    protected function validateSheets(): bool
    {
        $sheetNames = $this->spreadsheet->getSheetNames();

        $requiredSheets = [self::SHEET_SUMMARY, self::SHEET_MONTHLY, self::SHEET_ARREARS];
        $missingSheets = [];

        foreach ($requiredSheets as $required) {
            $found = false;
            foreach ($sheetNames as $name) {
                if (stripos($name, $required) !== false || stripos($required, $name) !== false) {
                    $found = true;
                    break;
                }
            }
            if (!$found) {
                $missingSheets[] = $required;
            }
        }

        if (!empty($missingSheets)) {
            $this->errors[] = 'Missing required sheets: ' . implode(', ', $missingSheets);
            return false;
        }

        return true;
    }

    /**
     * Find sheet by partial name match
     */
    protected function findSheet(string $searchName): ?Worksheet
    {
        foreach ($this->spreadsheet->getSheetNames() as $name) {
            if (stripos($name, $searchName) !== false) {
                return $this->spreadsheet->getSheetByName($name);
            }
        }
        return null;
    }

    /**
     * Parse Summary sheet
     */
    protected function parseSummarySheet(): void
    {
        $sheet = $this->findSheet('Summary');
        if (!$sheet) {
            return;
        }

        $this->parsedData['summary'] = [
            'employer_name'              => $this->getCellValue($sheet, 'C5'),
            'employer_number'            => $this->getCellValue($sheet, 'C6'),
            'start_date'                 => $this->parseDate($this->getCellValue($sheet, 'C7')),
            'end_date'                   => $this->parseDate($this->getCellValue($sheet, 'C8')),
            'financial_year'             => $this->getCellValue($sheet, 'C9'),
            'num_employees'              => $this->getNumericValue($sheet, 'C10'),
            'total_basic_wages'          => $this->getNumericValue($sheet, 'C12'),
            'non_resident_salaries'      => $this->getNumericValue($sheet, 'C13'),
            'gross_earnings'             => $this->getNumericValue($sheet, 'C14'),
            'contributions_due'          => $this->getNumericValue($sheet, 'C16'),
            'special_contribution_due'   => $this->getNumericValue($sheet, 'C17'),
            'contributions_paid'         => $this->getNumericValue($sheet, 'C19'),
            'special_contribution_paid'  => $this->getNumericValue($sheet, 'C20'),
            'contribution_arrears'       => $this->getNumericValue($sheet, 'C22'),
            'special_contribution_arrears' => $this->getNumericValue($sheet, 'C23'),
            'penalty_on_arrears'         => $this->getNumericValue($sheet, 'C25'),
            'interest_rate'              => $this->getNumericValue($sheet, 'C26'),
            'interest_on_arrears'        => $this->getNumericValue($sheet, 'C27'),
            'arrears_discovered_15'      => $this->getNumericValue($sheet, 'C30'),
            'special_contribution_10'    => $this->getNumericValue($sheet, 'C31'),
            'total_interest'             => $this->getNumericValue($sheet, 'C33'),
            'outstanding_arrears_interest' => $this->getNumericValue($sheet, 'C34'),
            'total_penalty'              => $this->getNumericValue($sheet, 'C35'),
            'total_amount'               => $this->getNumericValue($sheet, 'C36'),
        ];

        // Validate essential fields
        if (empty($this->parsedData['summary']['employer_name'])) {
            $this->errors[] = 'Employer name not found in Summary sheet';
        }
        if (empty($this->parsedData['summary']['employer_number'])) {
            $this->errors[] = 'Employer number not found in Summary sheet';
        }
    }

    /**
     * Parse Monthly Totals sheet
     */
    protected function parseMonthlySheet(): void
    {
        $sheet = $this->findSheet('Monthly');
        if (!$sheet) {
            return;
        }

        $this->parsedData['monthly'] = [];
        $highestRow = $sheet->getHighestRow();

        // Start from row 3 (assuming row 1-2 are headers)
        for ($row = 3; $row <= $highestRow; $row++) {
            $monthYear = $this->getCellValue($sheet, "A{$row}");

            // Skip empty rows
            if (empty($monthYear)) {
                continue;
            }

            $this->parsedData['monthly'][] = [
                'month_year'              => $this->parseDate($monthYear),
                'num_employees'           => $this->getNumericValue($sheet, "B{$row}"),
                'salaries_wages'          => $this->getNumericValue($sheet, "C{$row}"),
                'non_resident_salaries'   => $this->getNumericValue($sheet, "D{$row}"),
                'additional_cash_payments' => $this->getNumericValue($sheet, "E{$row}"),
                'gross_earnings'          => $this->getNumericValue($sheet, "F{$row}"),
                'standard_contribution'   => $this->getNumericValue($sheet, "G{$row}"),
                'special_contribution'    => $this->getNumericValue($sheet, "H{$row}"),
                'contributions_deductable' => $this->getNumericValue($sheet, "I{$row}"),
                'standard_remitted'       => $this->getNumericValue($sheet, "J{$row}"),
                'special_remitted'        => $this->getNumericValue($sheet, "K{$row}"),
                'due_date'                => $this->parseDate($this->getCellValue($sheet, "L{$row}")),
                'arrears_15'              => $this->getNumericValue($sheet, "M{$row}"),
                'arrears_10'              => $this->getNumericValue($sheet, "N{$row}"),
                'total_arrears'           => $this->getNumericValue($sheet, "O{$row}"),
                'months_in_arrears'       => $this->getNumericValue($sheet, "P{$row}"),
                'penalty'                 => $this->getNumericValue($sheet, "Q{$row}"),
            ];
        }
    }

    /**
     * Parse Arrears-Employee Details sheet
     */
    protected function parseArrearsSheet(): void
    {
        $sheet = $this->findSheet('Arrears');
        if (!$sheet) {
            return;
        }

        $this->parsedData['arrears'] = [];
        $highestRow = $sheet->getHighestRow();

        // Start from row 3 (assuming row 1-2 are headers)
        for ($row = 3; $row <= $highestRow; $row++) {
            $nssfNumber = $this->getCellValue($sheet, "A{$row}");

            // Skip empty rows
            if (empty($nssfNumber)) {
                continue;
            }

            $this->parsedData['arrears'][] = [
                'nssf_number'        => $nssfNumber,
                'contribution_type'  => $this->getCellValue($sheet, "B{$row}"),
                'contribution_year'  => $this->getNumericValue($sheet, "C{$row}"),
                'contribution_month' => $this->getNumericValue($sheet, "D{$row}"),
                'employee_name'      => $this->getCellValue($sheet, "E{$row}"),
                'employee_gross_pay' => $this->getNumericValue($sheet, "F{$row}"),
                'nssf_contribution'  => $this->getNumericValue($sheet, "G{$row}"),
                'remitted'           => $this->getNumericValue($sheet, "H{$row}"),
                'arrears'            => $this->getNumericValue($sheet, "I{$row}"),
                'narration'          => $this->getCellValue($sheet, "J{$row}"),
            ];
        }
    }

    /**
     * Get cell value safely
     */
    protected function getCellValue(Worksheet $sheet, string $cell): mixed
    {
        try {
            $value = $sheet->getCell($cell)->getValue();
            return is_string($value) ? trim($value) : $value;
        } catch (\Exception $e) {
            return null;
        }
    }

    /**
     * Get numeric value from cell
     */
    protected function getNumericValue(Worksheet $sheet, string $cell): ?float
    {
        $value = $this->getCellValue($sheet, $cell);
        if ($value === null || $value === '') {
            return null;
        }
        return is_numeric($value) ? (float) $value : null;
    }

    /**
     * Parse date value
     */
    protected function parseDate($value): ?string
    {
        if (empty($value)) {
            return null;
        }

        // If it's an Excel serial date number
        if (is_numeric($value)) {
            try {
                $date = \PhpOffice\PhpSpreadsheet\Shared\Date::excelToDateTimeObject($value);
                return $date->format('Y-m-d');
            } catch (\Exception $e) {
                return null;
            }
        }

        // Try to parse as date string
        try {
            $date = new \DateTime($value);
            return $date->format('Y-m-d');
        } catch (\Exception $e) {
            return null;
        }
    }

    /**
     * Get parsed data
     */
    public function getParsedData(): array
    {
        return $this->parsedData;
    }

    /**
     * Get summary data
     */
    public function getSummary(): array
    {
        return $this->parsedData['summary'] ?? [];
    }

    /**
     * Get monthly data
     */
    public function getMonthly(): array
    {
        return $this->parsedData['monthly'] ?? [];
    }

    /**
     * Get arrears data
     */
    public function getArrears(): array
    {
        return $this->parsedData['arrears'] ?? [];
    }

    /**
     * Get validation errors
     */
    public function getErrors(): array
    {
        return $this->errors;
    }

    /**
     * Check if parsing was successful
     */
    public function isValid(): bool
    {
        return empty($this->errors);
    }

    /**
     * Get totals for display
     */
    public function getTotals(): array
    {
        $summary = $this->getSummary();
        return [
            'standard_arrears' => $summary['contribution_arrears'] ?? 0,
            'interest'         => $summary['total_interest'] ?? 0,
            'penalty'          => $summary['total_penalty'] ?? 0,
            'total_amount'     => $summary['total_amount'] ?? 0,
        ];
    }
}
