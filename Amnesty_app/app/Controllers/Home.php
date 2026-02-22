<?php

namespace App\Controllers;

class Home extends BaseController
{
    /**
     * Landing page
     */
    public function index(): string
    {
        return view('home/index', [
            'title' => 'NSSF Amnesty Campaign - Welcome'
        ]);
    }

    /**
     * Download SAT template file
     */
    public function downloadTemplate()
    {
        $templatePath = FCPATH . 'assets/sat-template/NSSF Self Assessement Tool - FY 2025-26.xlsm';

        if (!file_exists($templatePath)) {
            return $this->response->setStatusCode(404)->setBody('Template file not found');
        }

        return $this->response->download($templatePath, null)->setFileName('NSSF_SAT_Template_FY2025-26.xlsm');
    }

    /**
     * Accept terms and conditions (AJAX)
     */
    public function acceptTerms()
    {
        if (!$this->request->isAJAX()) {
            return redirect()->to('/');
        }

        session()->set('terms_accepted', true);
        session()->set('terms_accepted_at', date('Y-m-d H:i:s'));

        return $this->response->setJSON([
            'success' => true,
            'redirect' => base_url('apply')
        ]);
    }

    /**
     * Check if terms are accepted (for AJAX checks)
     */
    public function checkTerms()
    {
        return $this->response->setJSON([
            'accepted' => session()->get('terms_accepted') === true
        ]);
    }
}
