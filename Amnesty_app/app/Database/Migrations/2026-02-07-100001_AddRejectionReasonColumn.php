<?php

namespace App\Database\Migrations;

use CodeIgniter\Database\Migration;

class AddRejectionReasonColumn extends Migration
{
    public function up()
    {
        $this->forge->addColumn('amnesty_applications', [
            'rejection_reason' => [
                'type'  => 'TEXT',
                'null'  => true,
                'after' => 'status',
            ],
        ]);
    }

    public function down()
    {
        $this->forge->dropColumn('amnesty_applications', 'rejection_reason');
    }
}
