<?php

namespace App\Database\Migrations;

use CodeIgniter\Database\Migration;

class CreateSatMonthlyTotals extends Migration
{
    public function up()
    {
        $this->forge->addField([
            'id' => [
                'type'           => 'INT',
                'constraint'     => 11,
                'unsigned'       => true,
                'auto_increment' => true,
            ],
            'application_id' => [
                'type'       => 'INT',
                'constraint' => 11,
                'unsigned'   => true,
            ],
            'month_year' => [
                'type' => 'DATE',
                'null' => true,
            ],
            'num_employees' => [
                'type'       => 'INT',
                'constraint' => 11,
                'null'       => true,
            ],
            'salaries_wages' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'non_resident_salaries' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'additional_cash_payments' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'gross_earnings' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'standard_contribution' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'special_contribution' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'contributions_deductable' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'standard_remitted' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'special_remitted' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'due_date' => [
                'type' => 'DATE',
                'null' => true,
            ],
            'arrears_15' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'arrears_10' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'total_arrears' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'months_in_arrears' => [
                'type'       => 'INT',
                'constraint' => 11,
                'null'       => true,
            ],
            'penalty' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'created_at' => [
                'type' => 'DATETIME',
                'null' => true,
            ],
            'updated_at' => [
                'type' => 'DATETIME',
                'null' => true,
            ],
        ]);

        $this->forge->addKey('id', true);
        $this->forge->addKey('application_id');
        $this->forge->addKey('month_year');
        $this->forge->addForeignKey('application_id', 'amnesty_applications', 'id', 'CASCADE', 'CASCADE');
        $this->forge->createTable('sat_monthly_totals');
    }

    public function down()
    {
        $this->forge->dropTable('sat_monthly_totals');
    }
}
