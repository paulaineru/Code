<?php

namespace App\Database\Migrations;

use CodeIgniter\Database\Migration;

class CreateAmnestyApplications extends Migration
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
            'employer_number' => [
                'type'       => 'VARCHAR',
                'constraint' => 50,
            ],
            'employer_name' => [
                'type'       => 'VARCHAR',
                'constraint' => 255,
            ],
            'email' => [
                'type'       => 'VARCHAR',
                'constraint' => 255,
            ],
            'phone' => [
                'type'       => 'VARCHAR',
                'constraint' => 20,
            ],
            'sector' => [
                'type'       => 'VARCHAR',
                'constraint' => 100,
            ],
            'standard_arrears' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'interest' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'penalty' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'amount_paid_standard' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'amount_paid_interest' => [
                'type'       => 'DECIMAL',
                'constraint' => '15,2',
                'null'       => true,
            ],
            'period_from' => [
                'type' => 'DATE',
                'null' => true,
            ],
            'period_to' => [
                'type' => 'DATE',
                'null' => true,
            ],
            'sat_file_path' => [
                'type'       => 'VARCHAR',
                'constraint' => 500,
                'null'       => true,
            ],
            'payment_reference' => [
                'type'       => 'VARCHAR',
                'constraint' => 100,
                'null'       => true,
            ],
            'payment_proof_path' => [
                'type'       => 'VARCHAR',
                'constraint' => 500,
                'null'       => true,
            ],
            'terms_accepted' => [
                'type'       => 'TINYINT',
                'constraint' => 1,
                'default'    => 0,
            ],
            'terms_accepted_at' => [
                'type' => 'DATETIME',
                'null' => true,
            ],
            'status' => [
                'type'       => 'ENUM',
                'constraint' => ['submitted', 'under_review', 'accepted', 'rejected'],
                'default'    => 'submitted',
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
        $this->forge->addKey('employer_number');
        $this->forge->addKey('status');
        $this->forge->addKey('created_at');
        $this->forge->createTable('amnesty_applications');
    }

    public function down()
    {
        $this->forge->dropTable('amnesty_applications');
    }
}
