<?php

namespace App\Database\Migrations;

use CodeIgniter\Database\Migration;

class CreateSyncLog extends Migration
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
            'sync_type' => [
                'type'       => 'ENUM',
                'constraint' => ['api', 'file_sat', 'file_payment'],
            ],
            'status' => [
                'type'       => 'ENUM',
                'constraint' => ['pending', 'in_progress', 'synced', 'failed'],
                'default'    => 'pending',
            ],
            'remote_id' => [
                'type'       => 'VARCHAR',
                'constraint' => 100,
                'null'       => true,
            ],
            'attempts' => [
                'type'       => 'INT',
                'constraint' => 11,
                'default'    => 0,
            ],
            'last_attempt_at' => [
                'type' => 'DATETIME',
                'null' => true,
            ],
            'error_message' => [
                'type' => 'TEXT',
                'null' => true,
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
        $this->forge->addKey('status');
        $this->forge->addKey('sync_type');
        $this->forge->addForeignKey('application_id', 'amnesty_applications', 'id', 'CASCADE', 'CASCADE');
        $this->forge->createTable('sync_log');
    }

    public function down()
    {
        $this->forge->dropTable('sync_log');
    }
}
