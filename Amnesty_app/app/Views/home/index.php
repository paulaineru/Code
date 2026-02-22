<?= $this->extend('layouts/main') ?>

<?= $this->section('content') ?>

<div class="row justify-content-center">
    <div class="col-lg-10">
        <!-- Hero Section -->
        <div class="card card-hero mb-4">
            <div class="card-body text-center">
                <h1 class="display-5 fw-bold mb-3">
                    <i class="bi bi-shield-check"></i> NSSF Amnesty Campaign
                </h1>
                <p class="lead mb-4">
                    Regularize your employer contributions and clear outstanding arrears
                </p>
                <div class="d-flex justify-content-center gap-3 flex-wrap">
                    <a href="<?= base_url('download-template') ?>" class="btn btn-nssf-outline btn-lg">
                        <i class="bi bi-download me-2"></i>Download SAT Template
                    </a>
                    <button type="button" class="btn btn-nssf btn-lg" data-bs-toggle="modal" data-bs-target="#termsModal">
                        <i class="bi bi-arrow-right-circle me-2"></i>Start Application
                    </button>
                </div>
            </div>
        </div>

        <!-- Instructions Section -->
        <div class="row g-4 mb-4">
            <div class="col-md-4">
                <div class="card card-feature h-100">
                    <div class="card-body text-center">
                        <div class="display-4">
                            <i class="bi bi-1-circle-fill"></i>
                        </div>
                        <h5 class="card-title fw-bold">Download Template</h5>
                        <p class="card-text text-muted">
                            Download the Self Assessment Tool (SAT) Excel template and fill in your contribution details.
                        </p>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card card-feature h-100">
                    <div class="card-body text-center">
                        <div class="display-4">
                            <i class="bi bi-2-circle-fill"></i>
                        </div>
                        <h5 class="card-title fw-bold">Complete Application</h5>
                        <p class="card-text text-muted">
                            Upload your completed SAT file, provide employer details, and submit proof of payment.
                        </p>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card card-feature h-100">
                    <div class="card-body text-center">
                        <div class="display-4">
                            <i class="bi bi-3-circle-fill"></i>
                        </div>
                        <h5 class="card-title fw-bold">Submit & Track</h5>
                        <p class="card-text text-muted">
                            Submit your application and receive a reference number to track your amnesty status.
                        </p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Eligibility Section -->
        <div class="card card-info mb-4">
            <div class="card-header">
                <h5 class="mb-0"><i class="bi bi-info-circle me-2"></i>Eligibility Criteria</h5>
            </div>
            <div class="card-body">
                <ul class="list-unstyled mb-0">
                    <li class="mb-2"><i class="bi bi-check-circle text-success me-2"></i>All employers registered with NSSF</li>
                    <li class="mb-2"><i class="bi bi-check-circle text-success me-2"></i>Outstanding contribution arrears from any period</li>
                    <li class="mb-2"><i class="bi bi-check-circle text-success me-2"></i>Interest and penalties accrued on arrears</li>
                    <li><i class="bi bi-check-circle text-success me-2"></i>Voluntary disclosure of previously unreported contributions</li>
                </ul>
            </div>
        </div>

        <!-- Required Documents -->
        <div class="card card-info mb-4">
            <div class="card-header">
                <h5 class="mb-0"><i class="bi bi-folder me-2"></i>Required Documents</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6">
                        <ul class="list-unstyled mb-0">
                            <li class="mb-2"><i class="bi bi-file-earmark-excel text-success me-2"></i>Completed SAT Excel file (.xlsm)</li>
                            <li class="mb-2"><i class="bi bi-file-earmark-pdf text-danger me-2"></i>Proof of payment (PDF, JPG, or PNG)</li>
                        </ul>
                    </div>
                    <div class="col-md-6">
                        <ul class="list-unstyled mb-0">
                            <li class="mb-2"><i class="bi bi-hash me-2"></i>Payment reference/TRN number</li>
                            <li><i class="bi bi-building me-2"></i>Valid employer registration number</li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>

        <!-- Check Application Status -->
        <div class="card card-info">
            <div class="card-header">
                <h5 class="mb-0"><i class="bi bi-search me-2"></i>Check Application Status</h5>
            </div>
            <div class="card-body">
                <p class="text-muted mb-3">Already submitted an application? Enter your reference number to check the status.</p>
                <form id="statusCheckForm" class="row g-2 align-items-center">
                    <div class="col-md-8">
                        <input type="text" class="form-control" id="referenceNumber"
                               placeholder="Enter reference number (e.g., AMN-2026-XXXXXX)"
                               pattern="AMN-\d{4}-[A-Z0-9]+" required>
                    </div>
                    <div class="col-md-4">
                        <button type="submit" class="btn btn-nssf w-100">
                            <i class="bi bi-search me-2"></i>Check Status
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

<!-- Terms & Conditions Modal -->
<div class="modal fade" id="termsModal" tabindex="-1" aria-labelledby="termsModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg modal-dialog-scrollable">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="termsModalLabel">
                    <i class="bi bi-file-text me-2"></i>Terms & Conditions
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <h6>NSSF Amnesty Campaign Terms and Conditions</h6>
                <p>By proceeding with this application, you acknowledge and agree to the following:</p>

                <ol>
                    <li class="mb-3">
                        <strong>Accuracy of Information</strong><br>
                        All information provided in this application, including the Self Assessment Tool (SAT) data, is true, accurate, and complete to the best of my knowledge.
                    </li>
                    <li class="mb-3">
                        <strong>Verification Rights</strong><br>
                        NSSF reserves the right to verify all information submitted and may request additional documentation if necessary.
                    </li>
                    <li class="mb-3">
                        <strong>Payment Commitment</strong><br>
                        I commit to making the full payment of standard arrears and applicable interest as calculated in the SAT within the amnesty period.
                    </li>
                    <li class="mb-3">
                        <strong>Waiver of Penalties</strong><br>
                        Upon successful verification and payment, penalties on arrears may be waived as per the amnesty program guidelines.
                    </li>
                    <li class="mb-3">
                        <strong>Non-Compliance Consequences</strong><br>
                        Failure to complete payment within the specified period will result in the reinstatement of all penalties and interest.
                    </li>
                    <li class="mb-3">
                        <strong>Data Privacy</strong><br>
                        All personal and employer data submitted will be handled in accordance with applicable data protection laws and NSSF's privacy policy.
                    </li>
                    <li class="mb-3">
                        <strong>Amendment Rights</strong><br>
                        NSSF reserves the right to amend these terms and conditions at any time without prior notice.
                    </li>
                    <li>
                        <strong>Legal Compliance</strong><br>
                        This application does not exempt the employer from any other legal obligations under the NSSF Act and related regulations.
                    </li>
                </ol>

                <div class="form-check mt-4">
                    <input class="form-check-input" type="checkbox" id="acceptTermsCheck">
                    <label class="form-check-label" for="acceptTermsCheck">
                        I have read, understood, and agree to the Terms and Conditions of the NSSF Amnesty Campaign.
                    </label>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-nssf" id="acceptTermsBtn" disabled>
                    <i class="bi bi-check-circle me-2"></i>Accept & Proceed
                </button>
            </div>
        </div>
    </div>
</div>

<?= $this->endSection() ?>

<?= $this->section('scripts') ?>
<script>
    document.addEventListener('DOMContentLoaded', function() {
        const acceptCheck = document.getElementById('acceptTermsCheck');
        const acceptBtn = document.getElementById('acceptTermsBtn');

        // Enable/disable accept button based on checkbox
        acceptCheck.addEventListener('change', function() {
            acceptBtn.disabled = !this.checked;
        });

        // Handle status check form
        const statusForm = document.getElementById('statusCheckForm');
        statusForm.addEventListener('submit', function(e) {
            e.preventDefault();
            const refNumber = document.getElementById('referenceNumber').value.trim();
            if (refNumber) {
                window.location.href = '<?= base_url('apply/status') ?>/' + encodeURIComponent(refNumber);
            }
        });

        // Handle accept button click
        acceptBtn.addEventListener('click', function() {
            showLoading();

            fetch('<?= base_url('accept-terms') ?>', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    '<?= csrf_token() ?>': '<?= csrf_hash() ?>'
                }
            })
            .then(response => response.json())
            .then(data => {
                hideLoading();
                if (data.success) {
                    window.location.href = data.redirect;
                } else {
                    showToast('Failed to accept terms. Please try again.', 'error');
                }
            })
            .catch(error => {
                hideLoading();
                showToast('An error occurred. Please try again.', 'error');
                console.error('Error:', error);
            });
        });
    });
</script>
<?= $this->endSection() ?>
