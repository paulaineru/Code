<?php

namespace App\Database\Migrations;

use CodeIgniter\Database\Migration;

class CreateSatArrearsEmployees extends Migration
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
            'nssf_number' => [
                'type'       => 'VARCHAR',
                'constraint' => 50,
                'null'       => true,
            ],
            'contribution_type' => [
                'type'       => 'VARCHAR',
                'constraint' => 50,
                'null'       => true,
            ],
            'contribution_year' => [
                'type'       => 'INT',
                'constraint' => 4,
                'null'       => true,
            ],
            'contribution_month' => [
                'type'       => 'INT',
                'constraint' => 2,
                'null'       => true,
            ],
            'employee_name' => [
                'type'       => 'VARCHAR',
                'constraint' => 255,
                'null'       => true,
            ],
            'employee_gross_pay' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'nssf_contribution' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'remitted' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'arrears' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'narration' => [
                'type'       => 'VARCHAR',
                'constraint' => 100,
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
        $this->forge->addKey('nssf_number');
        $this->forge->addKey(['contribution_year', 'contribution_month']);
        $this->forge->addForeignKey('application_id', 'amnesty_applications', 'id', 'CASCADE', 'CASCADE');
        $this->forge->createTable('sat_arrears_employees');
    }

    public function down()
    {
        $this->forge->dropTable('sat_arrears_employees');
    }
}
