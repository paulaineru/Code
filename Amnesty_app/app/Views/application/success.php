<?= $this->extend('layouts/main') ?>

<?= $this->section('content') ?>

<div class="row justify-content-center">
    <div class="col-lg-8">
        <div class="card card-success-result text-center">
            <div class="card-body py-5">
                <div class="display-1 mb-4" style="color: var(--nssf-accent);">
                    <i class="bi bi-check-circle-fill"></i>
                </div>

                <h2 class="mb-3" style="color: var(--nssf-primary);">Application Submitted Successfully!</h2>

                <p class="lead text-muted mb-4">
                    Your amnesty application has been received and is now under review.
                </p>

                <div class="card card-reference mb-4 mx-auto" style="max-width: 400px;">
                    <div class="card-body">
                        <h5 class="card-title mb-2">Reference Number</h5>
                        <p class="display-6 fw-bold mb-1"><?= esc($reference) ?></p>
                        <small style="color: rgba(255,255,255,0.7);">Please keep this number for your records</small>
                    </div>
                </div>

                <div class="alert alert-info text-start mx-auto" style="max-width: 500px;">
                    <h6><i class="bi bi-info-circle me-2"></i>What happens next?</h6>
                    <ol class="mb-0 ps-3">
                        <li>Our team will review your application and SAT data</li>
                        <li>We will verify your payment proof</li>
                        <li>You will receive an email notification once reviewed</li>
                        <li>If accepted, your arrears will be regularized</li>
                    </ol>
                </div>

                <hr class="my-4">

                <div class="d-flex justify-content-center gap-3 flex-wrap">
                    <a href="<?= base_url() ?>" class="btn btn-nssf-outline">
                        <i class="bi bi-house me-2"></i>Return to Home
                    </a>
                    <a href="<?= base_url() ?>" class="btn btn-nssf">
                        <i class="bi bi-plus-circle me-2"></i>Submit Another Application
                    </a>
                </div>
            </div>
        </div>
    </div>
</div>

<?= $this->endSection() ?>
