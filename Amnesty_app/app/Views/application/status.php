<?= $this->extend('layouts/main') ?>

<?= $this->section('content') ?>

<div class="row justify-content-center">
    <div class="col-lg-8">
        <?php if (!$found): ?>
        <div class="card text-center">
            <div class="card-body py-5">
                <div class="display-1 mb-4 text-muted">
                    <i class="bi bi-question-circle"></i>
                </div>

                <h2 class="mb-3">Application Not Found</h2>

                <p class="text-muted mb-4">
                    We couldn't find an application with reference number:<br>
                    <strong class="fs-5"><?= esc($reference) ?></strong>
                </p>

                <div class="alert alert-warning text-start mx-auto" style="max-width: 500px;">
                    <h6><i class="bi bi-exclamation-triangle me-2"></i>Please check:</h6>
                    <ul class="mb-0 ps-3">
                        <li>The reference number is typed correctly</li>
                        <li>The reference number includes all dashes (e.g., AMN-2026-XXXXXX)</li>
                    </ul>
                </div>

                <hr class="my-4">

                <a href="<?= base_url() ?>" class="btn btn-nssf">
                    <i class="bi bi-house me-2"></i>Return to Home
                </a>
            </div>
        </div>
        <?php else: ?>
        <div class="card">
            <div class="card-body py-5">
                <div class="text-center mb-4">
                    <h2 style="color: var(--nssf-primary);">Application Status</h2>
                    <p class="text-muted">Reference: <strong><?= esc($reference) ?></strong></p>
                </div>

                <div class="row g-4">
                    <div class="col-md-6">
                        <div class="card h-100">
                            <div class="card-header">
                                <h5 class="mb-0"><i class="bi bi-building me-2"></i>Employer Details</h5>
                            </div>
                            <div class="card-body">
                                <p class="mb-2"><strong>Name:</strong> <?= esc($application['employer_name']) ?></p>
                                <p class="mb-2"><strong>Number:</strong> <?= esc($application['employer_number']) ?></p>
                                <p class="mb-0"><strong>Sector:</strong> <?= esc($application['sector']) ?></p>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-6">
                        <div class="card h-100">
                            <div class="card-header">
                                <h5 class="mb-0"><i class="bi bi-clock-history me-2"></i>Status</h5>
                            </div>
                            <div class="card-body">
                                <?php
                                $statusColors = [
                                    'submitted'    => 'primary',
                                    'under_review' => 'warning',
                                    'accepted'     => 'success',
                                    'rejected'     => 'danger',
                                ];
                                $statusLabels = [
                                    'submitted'    => 'Submitted',
                                    'under_review' => 'Under Review',
                                    'accepted'     => 'Accepted',
                                    'rejected'     => 'Rejected',
                                ];
                                $status = $application['status'];
                                $color = $statusColors[$status] ?? 'secondary';
                                $label = $statusLabels[$status] ?? ucfirst($status);
                                ?>
                                <p class="mb-2">
                                    <span class="badge bg-<?= $color ?> fs-6"><?= $label ?></span>
                                </p>
                                <p class="mb-2"><strong>Submitted:</strong> <?= date('d M Y, H:i', strtotime($application['created_at'])) ?></p>
                                <?php if ($application['updated_at'] !== $application['created_at']): ?>
                                <p class="mb-0"><strong>Last Updated:</strong> <?= date('d M Y, H:i', strtotime($application['updated_at'])) ?></p>
                                <?php endif; ?>

                                <?php if ($status === 'rejected' && !empty($application['rejection_reason'])): ?>
                                <hr class="my-3">
                                <div class="alert alert-danger mb-0">
                                    <h6 class="alert-heading">
                                        <i class="bi bi-exclamation-triangle me-2"></i>Reason for Rejection
                                    </h6>
                                    <p class="mb-0"><?= esc($application['rejection_reason']) ?></p>
                                </div>
                                <?php endif; ?>
                            </div>
                        </div>
                    </div>

                    <div class="col-12">
                        <div class="card">
                            <div class="card-header">
                                <h5 class="mb-0"><i class="bi bi-cash-stack me-2"></i>Payment Summary</h5>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-md-6">
                                        <p class="mb-2"><strong>Standard Arrears:</strong> UGX <?= number_format($application['standard_arrears'], 2) ?></p>
                                        <p class="mb-2"><strong>Interest:</strong> UGX <?= number_format($application['interest'], 2) ?></p>
                                    </div>
                                    <div class="col-md-6">
                                        <p class="mb-2"><strong>Amount Paid (Standard):</strong> UGX <?= number_format($application['amount_paid_standard'], 2) ?></p>
                                        <p class="mb-2"><strong>Amount Paid (Interest):</strong> UGX <?= number_format($application['amount_paid_interest'], 2) ?></p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <hr class="my-4">

                <div class="text-center">
                    <a href="<?= base_url() ?>" class="btn btn-nssf-outline">
                        <i class="bi bi-house me-2"></i>Return to Home
                    </a>
                </div>
            </div>
        </div>
        <?php endif; ?>
    </div>
</div>

<?= $this->endSection() ?>
