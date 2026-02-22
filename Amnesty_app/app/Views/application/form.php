<?= $this->extend('layouts/main') ?>

<?= $this->section('content') ?>

<div class="row justify-content-center">
    <div class="col-lg-10">
        <div class="card card-info">
            <div class="card-header">
                <h4 class="mb-0"><i class="bi bi-file-earmark-text me-2"></i>Amnesty Application Form</h4>
            </div>
            <div class="card-body">
                <!-- Step Indicator -->
                <div class="step-indicator mb-4">
                    <div class="step active" data-step="1">
                        <span class="step-number">1</span>
                        <div class="step-label small mt-2">Upload SAT</div>
                    </div>
                    <div class="step" data-step="2">
                        <span class="step-number">2</span>
                        <div class="step-label small mt-2">Employer Details</div>
                    </div>
                    <div class="step" data-step="3">
                        <span class="step-number">3</span>
                        <div class="step-label small mt-2">Payment Proof</div>
                    </div>
                    <div class="step" data-step="4">
                        <span class="step-number">4</span>
                        <div class="step-label small mt-2">Review & Submit</div>
                    </div>
                </div>

                <form id="applicationForm">
                    <?= csrf_field() ?>

                    <!-- Step 1: SAT Upload -->
                    <div class="form-step active" id="step1">
                        <h5 class="mb-4">Step 1: Upload Self Assessment Tool (SAT)</h5>

                        <div class="alert alert-info">
                            <i class="bi bi-info-circle me-2"></i>
                            Upload your completed SAT Excel file. The system will automatically extract and validate the data.
                        </div>

                        <div class="mb-4">
                            <label class="form-label">SAT File (.xlsm, .xlsx, .xls)</label>
                            <input type="file" class="form-control" id="satFile" accept=".xlsm,.xlsx,.xls" required>
                            <div class="form-text">Maximum file size: 10MB</div>
                        </div>

                        <button type="button" class="btn btn-nssf" id="uploadSatBtn">
                            <i class="bi bi-upload me-2"></i>Upload & Parse
                        </button>

                        <!-- Parsed Data Preview -->
                        <div id="satPreview" class="mt-4" style="display: none;">
                            <h6 class="text-success"><i class="bi bi-check-circle me-2"></i>SAT Data Extracted Successfully</h6>
                            <div class="table-responsive">
                                <table class="table table-sm table-bordered">
                                    <tr><th width="200">Employer Name</th><td id="previewEmployerName"></td></tr>
                                    <tr><th>Employer Number</th><td id="previewEmployerNumber"></td></tr>
                                    <tr><th>Period</th><td><span id="previewPeriodFrom"></span> to <span id="previewPeriodTo"></span></td></tr>
                                    <tr><th>Standard Arrears</th><td>UGX <span id="previewArrears"></span></td></tr>
                                    <tr><th>Interest</th><td>UGX <span id="previewInterest"></span></td></tr>
                                    <tr><th>Penalty</th><td>UGX <span id="previewPenalty"></span></td></tr>
                                    <tr class="table-primary"><th>Total Amount</th><td><strong>UGX <span id="previewTotal"></span></strong></td></tr>
                                </table>
                            </div>
                        </div>

                        <div id="satErrors" class="alert alert-danger mt-4" style="display: none;"></div>
                    </div>

                    <!-- Step 2: Employer Details -->
                    <div class="form-step" id="step2">
                        <h5 class="mb-4">Step 2: Employer Details</h5>

                        <div class="row g-3">
                            <div class="col-md-6">
                                <label class="form-label">Employer Number <span class="text-danger">*</span></label>
                                <input type="text" class="form-control" name="employer_number" id="employerNumber" required>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Employer Name <span class="text-danger">*</span></label>
                                <input type="text" class="form-control" name="employer_name" id="employerName" required>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Email Address <span class="text-danger">*</span></label>
                                <input type="email" class="form-control" name="email" id="email" required>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Phone Number <span class="text-danger">*</span></label>
                                <input type="tel" class="form-control" name="phone" id="phone" required>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Sector <span class="text-danger">*</span></label>
                                <select class="form-select" name="sector" id="sector" required>
                                    <option value="">-- Select Sector --</option>
                                    <?php foreach ($sectors as $sector): ?>
                                        <option value="<?= esc($sector) ?>"><?= esc($sector) ?></option>
                                    <?php endforeach; ?>
                                </select>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Period From</label>
                                <input type="date" class="form-control" name="period_from" id="periodFrom">
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Period To</label>
                                <input type="date" class="form-control" name="period_to" id="periodTo">
                            </div>
                        </div>

                        <hr class="my-4">
                        <h6>Payment Details</h6>

                        <div class="row g-3">
                            <div class="col-md-4">
                                <label class="form-label">Standard Arrears (UGX)</label>
                                <input type="text" class="form-control" name="standard_arrears" id="standardArrears" readonly>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label">Interest (UGX)</label>
                                <input type="text" class="form-control" name="interest" id="interest" readonly>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label">Penalty (UGX)</label>
                                <input type="text" class="form-control" name="penalty" id="penalty" readonly>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Amount Paid - Standard <span class="text-danger">*</span></label>
                                <input type="number" class="form-control" name="amount_paid_standard" id="amountPaidStandard" step="0.01" required>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Amount Paid - Interest <span class="text-danger">*</span></label>
                                <input type="number" class="form-control" name="amount_paid_interest" id="amountPaidInterest" step="0.01" required>
                            </div>
                        </div>
                    </div>

                    <!-- Step 3: Payment Proof -->
                    <div class="form-step" id="step3">
                        <h5 class="mb-4">Step 3: Payment Proof</h5>

                        <div class="row g-3">
                            <div class="col-md-6">
                                <label class="form-label">Payment Reference / TRN <span class="text-danger">*</span></label>
                                <input type="text" class="form-control" name="payment_reference" id="paymentReference" required>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Proof of Payment <span class="text-danger">*</span></label>
                                <input type="file" class="form-control" id="paymentProofFile" accept=".pdf,.jpg,.jpeg,.png" required>
                                <div class="form-text">Accepted formats: PDF, JPG, PNG (Max 5MB)</div>
                            </div>
                        </div>

                        <button type="button" class="btn btn-outline-primary mt-3" id="uploadPaymentBtn">
                            <i class="bi bi-upload me-2"></i>Upload Payment Proof
                        </button>

                        <div id="paymentUploadSuccess" class="alert alert-success mt-3" style="display: none;">
                            <i class="bi bi-check-circle me-2"></i>Payment proof uploaded: <span id="paymentFileName"></span>
                        </div>

                        <div id="paymentErrors" class="alert alert-danger mt-3" style="display: none;"></div>
                    </div>

                    <!-- Step 4: Review & Submit -->
                    <div class="form-step" id="step4">
                        <h5 class="mb-4">Step 4: Review & Submit</h5>

                        <div class="alert alert-warning">
                            <i class="bi bi-exclamation-triangle me-2"></i>
                            Please review all information carefully before submitting. Changes cannot be made after submission.
                        </div>

                        <div class="card card-review mb-3">
                            <div class="card-header"><i class="bi bi-building me-2"></i>Employer Information</div>
                            <div class="card-body">
                                <div class="row g-2">
                                    <div class="col-md-6"><small class="text-muted">Employer Number</small><br><strong id="reviewEmployerNumber"></strong></div>
                                    <div class="col-md-6"><small class="text-muted">Employer Name</small><br><strong id="reviewEmployerName"></strong></div>
                                    <div class="col-md-6"><small class="text-muted">Email</small><br><strong id="reviewEmail"></strong></div>
                                    <div class="col-md-6"><small class="text-muted">Phone</small><br><strong id="reviewPhone"></strong></div>
                                    <div class="col-md-6"><small class="text-muted">Sector</small><br><strong id="reviewSector"></strong></div>
                                </div>
                            </div>
                        </div>

                        <div class="card card-review mb-3">
                            <div class="card-header"><i class="bi bi-cash-stack me-2"></i>Payment Information</div>
                            <div class="card-body">
                                <div class="row g-2">
                                    <div class="col-md-4"><small class="text-muted">Standard Arrears</small><br><strong>UGX <span id="reviewArrears"></span></strong></div>
                                    <div class="col-md-4"><small class="text-muted">Interest</small><br><strong>UGX <span id="reviewInterest"></span></strong></div>
                                    <div class="col-md-4"><small class="text-muted">Penalty</small><br><strong>UGX <span id="reviewPenalty"></span></strong></div>
                                    <div class="col-md-6"><small class="text-muted">Amount Paid (Standard)</small><br><strong>UGX <span id="reviewPaidStandard"></span></strong></div>
                                    <div class="col-md-6"><small class="text-muted">Amount Paid (Interest)</small><br><strong>UGX <span id="reviewPaidInterest"></span></strong></div>
                                    <div class="col-md-12"><small class="text-muted">Payment Reference</small><br><strong id="reviewPaymentRef"></strong></div>
                                </div>
                            </div>
                        </div>

                        <div class="form-check mb-4">
                            <input class="form-check-input" type="checkbox" id="confirmSubmit" required>
                            <label class="form-check-label" for="confirmSubmit">
                                I confirm that all information provided is accurate and complete.
                            </label>
                        </div>
                    </div>

                    <!-- Navigation Buttons -->
                    <div class="d-flex justify-content-between mt-4 pt-3 border-top">
                        <button type="button" class="btn btn-outline-secondary" id="prevBtn" style="display: none;">
                            <i class="bi bi-arrow-left me-2"></i>Previous
                        </button>
                        <div class="ms-auto">
                            <button type="button" class="btn btn-nssf" id="nextBtn" disabled>
                                Next<i class="bi bi-arrow-right ms-2"></i>
                            </button>
                            <button type="button" class="btn btn-success" id="submitBtn" style="display: none;">
                                <i class="bi bi-check-circle me-2"></i>Submit Application
                            </button>
                        </div>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

<?= $this->endSection() ?>

<?= $this->section('scripts') ?>
<script>
    let currentStep = 1;
    const totalSteps = 4;
    let satUploaded = false;
    let paymentUploaded = false;

    // Format number with thousand separators
    function formatNumber(num) {
        return parseFloat(num).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    document.addEventListener('DOMContentLoaded', function() {
        updateStepDisplay();

        // SAT Upload
        document.getElementById('uploadSatBtn').addEventListener('click', uploadSat);

        // Payment Upload
        document.getElementById('uploadPaymentBtn').addEventListener('click', uploadPayment);

        // Navigation
        document.getElementById('prevBtn').addEventListener('click', prevStep);
        document.getElementById('nextBtn').addEventListener('click', nextStep);
        document.getElementById('submitBtn').addEventListener('click', submitApplication);

        // Enable next button when SAT file selected
        document.getElementById('satFile').addEventListener('change', function() {
            document.getElementById('uploadSatBtn').disabled = !this.files.length;
        });
    });

    function uploadSat() {
        const fileInput = document.getElementById('satFile');
        if (!fileInput.files.length) {
            showToast('Please select a SAT file', 'error');
            return;
        }

        const formData = new FormData();
        formData.append('sat_file', fileInput.files[0]);
        formData.append('<?= csrf_token() ?>', '<?= csrf_hash() ?>');

        showLoading();

        fetch('<?= base_url('apply/upload-sat') ?>', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => response.json())
        .then(data => {
            hideLoading();
            if (data.success) {
                // Show preview
                document.getElementById('satPreview').style.display = 'block';
                document.getElementById('satErrors').style.display = 'none';

                // Populate preview (formatted for display)
                document.getElementById('previewEmployerName').textContent = data.data.employer_name;
                document.getElementById('previewEmployerNumber').textContent = data.data.employer_number;
                document.getElementById('previewPeriodFrom').textContent = data.data.period_from;
                document.getElementById('previewPeriodTo').textContent = data.data.period_to;
                document.getElementById('previewArrears').textContent = formatNumber(data.data.standard_arrears);
                document.getElementById('previewInterest').textContent = formatNumber(data.data.interest);
                document.getElementById('previewPenalty').textContent = formatNumber(data.data.penalty);
                document.getElementById('previewTotal').textContent = formatNumber(data.data.total_amount);

                // Pre-fill form fields
                document.getElementById('employerNumber').value = data.data.employer_number;
                document.getElementById('employerName').value = data.data.employer_name;
                document.getElementById('periodFrom').value = data.data.period_from;
                document.getElementById('periodTo').value = data.data.period_to;
                document.getElementById('standardArrears').value = data.data.standard_arrears;
                document.getElementById('interest').value = data.data.interest;
                document.getElementById('penalty').value = data.data.penalty;

                satUploaded = true;
                updateNavButtons();
                showToast('SAT file uploaded and parsed successfully', 'success');
            } else {
                document.getElementById('satPreview').style.display = 'none';
                document.getElementById('satErrors').style.display = 'block';
                document.getElementById('satErrors').innerHTML = '<strong>Errors:</strong><br>' +
                    (data.errors ? data.errors.join('<br>') : data.error);
                satUploaded = false;
                updateNavButtons();
            }
        })
        .catch(error => {
            hideLoading();
            showToast('An error occurred while uploading the file', 'error');
            console.error('Error:', error);
        });
    }

    function uploadPayment() {
        const fileInput = document.getElementById('paymentProofFile');
        if (!fileInput.files.length) {
            showToast('Please select a payment proof file', 'error');
            return;
        }

        const formData = new FormData();
        formData.append('payment_proof', fileInput.files[0]);
        formData.append('<?= csrf_token() ?>', '<?= csrf_hash() ?>');

        showLoading();

        fetch('<?= base_url('apply/upload-payment') ?>', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => response.json())
        .then(data => {
            hideLoading();
            if (data.success) {
                document.getElementById('paymentUploadSuccess').style.display = 'block';
                document.getElementById('paymentFileName').textContent = data.filename;
                document.getElementById('paymentErrors').style.display = 'none';
                paymentUploaded = true;
                updateNavButtons();
                showToast('Payment proof uploaded successfully', 'success');
            } else {
                document.getElementById('paymentUploadSuccess').style.display = 'none';
                document.getElementById('paymentErrors').style.display = 'block';
                document.getElementById('paymentErrors').textContent = data.error;
                paymentUploaded = false;
                updateNavButtons();
            }
        })
        .catch(error => {
            hideLoading();
            showToast('An error occurred while uploading the file', 'error');
            console.error('Error:', error);
        });
    }

    function nextStep() {
        if (currentStep < totalSteps) {
            if (!validateCurrentStep()) {
                return;
            }
            currentStep++;
            updateStepDisplay();

            // Populate review on step 4
            if (currentStep === 4) {
                populateReview();
            }
        }
    }

    function prevStep() {
        if (currentStep > 1) {
            currentStep--;
            updateStepDisplay();
        }
    }

    function validateCurrentStep() {
        if (currentStep === 1 && !satUploaded) {
            showToast('Please upload and parse your SAT file first', 'error');
            return false;
        }

        if (currentStep === 2) {
            const requiredFields = ['employerNumber', 'employerName', 'email', 'phone', 'sector', 'amountPaidStandard', 'amountPaidInterest'];
            for (const field of requiredFields) {
                const input = document.getElementById(field);
                if (!input.value.trim()) {
                    showToast('Please fill in all required fields', 'error');
                    input.focus();
                    return false;
                }
            }

            // Validate that amounts paid match the arrears
            const standardArrears = parseFloat(document.getElementById('standardArrears').value) || 0;
            const amountPaidStandard = parseFloat(document.getElementById('amountPaidStandard').value) || 0;
            const interest = parseFloat(document.getElementById('interest').value) || 0;
            const amountPaidInterest = parseFloat(document.getElementById('amountPaidInterest').value) || 0;

            if (amountPaidStandard !== standardArrears) {
                showToast('Amount Paid - Standard must match Standard Arrears (UGX)', 'error');
                document.getElementById('amountPaidStandard').focus();
                return false;
            }

            if (amountPaidInterest !== interest) {
                showToast('Amount Paid - Interest must match Interest (UGX)', 'error');
                document.getElementById('amountPaidInterest').focus();
                return false;
            }
        }

        if (currentStep === 3) {
            if (!paymentUploaded) {
                showToast('Please upload your payment proof', 'error');
                return false;
            }
            if (!document.getElementById('paymentReference').value.trim()) {
                showToast('Please enter the payment reference number', 'error');
                return false;
            }
        }

        return true;
    }

    function updateStepDisplay() {
        // Update step content
        document.querySelectorAll('.form-step').forEach((step, index) => {
            step.classList.toggle('active', index + 1 === currentStep);
        });

        // Update step indicators
        document.querySelectorAll('.step').forEach((step, index) => {
            const stepNum = index + 1;
            step.classList.remove('active', 'completed');
            if (stepNum === currentStep) {
                step.classList.add('active');
            } else if (stepNum < currentStep) {
                step.classList.add('completed');
            }
        });

        updateNavButtons();
    }

    function updateNavButtons() {
        document.getElementById('prevBtn').style.display = currentStep > 1 ? 'inline-block' : 'none';
        document.getElementById('nextBtn').style.display = currentStep < totalSteps ? 'inline-block' : 'none';
        document.getElementById('submitBtn').style.display = currentStep === totalSteps ? 'inline-block' : 'none';

        // Enable/disable next button based on step
        let canProceed = true;
        if (currentStep === 1) canProceed = satUploaded;
        if (currentStep === 3) canProceed = paymentUploaded && document.getElementById('paymentReference').value.trim();

        document.getElementById('nextBtn').disabled = !canProceed;
    }

    function populateReview() {
        document.getElementById('reviewEmployerNumber').textContent = document.getElementById('employerNumber').value;
        document.getElementById('reviewEmployerName').textContent = document.getElementById('employerName').value;
        document.getElementById('reviewEmail').textContent = document.getElementById('email').value;
        document.getElementById('reviewPhone').textContent = document.getElementById('phone').value;
        document.getElementById('reviewSector').textContent = document.getElementById('sector').value;
        document.getElementById('reviewArrears').textContent = formatNumber(document.getElementById('standardArrears').value);
        document.getElementById('reviewInterest').textContent = formatNumber(document.getElementById('interest').value);
        document.getElementById('reviewPenalty').textContent = formatNumber(document.getElementById('penalty').value);
        document.getElementById('reviewPaidStandard').textContent = formatNumber(document.getElementById('amountPaidStandard').value);
        document.getElementById('reviewPaidInterest').textContent = formatNumber(document.getElementById('amountPaidInterest').value);
        document.getElementById('reviewPaymentRef').textContent = document.getElementById('paymentReference').value;
    }

    function submitApplication() {
        if (!document.getElementById('confirmSubmit').checked) {
            showToast('Please confirm that all information is accurate', 'error');
            return;
        }

        const formData = {
            employer_number: document.getElementById('employerNumber').value,
            employer_name: document.getElementById('employerName').value,
            email: document.getElementById('email').value,
            phone: document.getElementById('phone').value,
            sector: document.getElementById('sector').value,
            period_from: document.getElementById('periodFrom').value,
            period_to: document.getElementById('periodTo').value,
            standard_arrears: document.getElementById('standardArrears').value,
            interest: document.getElementById('interest').value,
            penalty: document.getElementById('penalty').value,
            amount_paid_standard: document.getElementById('amountPaidStandard').value,
            amount_paid_interest: document.getElementById('amountPaidInterest').value,
            payment_reference: document.getElementById('paymentReference').value
        };

        showLoading();

        fetch('<?= base_url('apply/submit') ?>', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest',
                '<?= csrf_token() ?>': '<?= csrf_hash() ?>'
            },
            body: JSON.stringify(formData)
        })
        .then(response => response.json())
        .then(data => {
            hideLoading();
            if (data.success) {
                window.location.href = data.redirect;
            } else {
                if (data.errors) {
                    showToast(Object.values(data.errors).join('<br>'), 'error');
                } else {
                    showToast(data.error || 'Failed to submit application', 'error');
                }
            }
        })
        .catch(error => {
            hideLoading();
            showToast('An error occurred while submitting the application', 'error');
            console.error('Error:', error);
        });
    }
</script>
<?= $this->endSection() ?>
