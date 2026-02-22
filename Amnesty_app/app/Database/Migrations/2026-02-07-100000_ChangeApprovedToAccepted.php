<?php

namespace App\Database\Migrations;

use CodeIgniter\Database\Migration;

class ChangeApprovedToAccepted extends Migration
{
    public function up()
    {
        // First, update any existing 'approved' rows to 'accepted'
        $this->db->query("UPDATE amnesty_applications SET status = 'accepted' WHERE status = 'approved'");

        // Then alter the ENUM to replace 'approved' with 'accepted'
        $this->db->query("ALTER TABLE amnesty_applications MODIFY COLUMN status ENUM('submitted', 'under_review', 'accepted', 'rejected') DEFAULT 'submitted'");
    }

    public function down()
    {
        // Revert: update 'accepted' back to 'approved'
        $this->db->query("UPDATE amnesty_applications SET status = 'approved' WHERE status = 'accepted'");

        // Revert the ENUM
        $this->db->query("ALTER TABLE amnesty_applications MODIFY COLUMN status ENUM('submitted', 'under_review', 'approved', 'rejected') DEFAULT 'submitted'");
    }
}
