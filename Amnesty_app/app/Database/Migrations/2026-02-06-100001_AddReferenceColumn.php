<?php

namespace App\Database\Migrations;

use CodeIgniter\Database\Migration;

class AddReferenceColumn extends Migration
{
    public function up()
    {
        $this->forge->addColumn('amnesty_applications', [
            'reference' => [
                'type'       => 'VARCHAR',
                'constraint' => 20,
                'null'       => true,
                'after'      => 'id',
            ],
        ]);

        // Add unique index
        $this->db->query('CREATE UNIQUE INDEX idx_reference ON amnesty_applications(reference)');
    }

    public function down()
    {
        $this->forge->dropColumn('amnesty_applications', 'reference');
    }
}
