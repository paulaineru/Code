<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title><?= $title ?? 'NSSF Amnesty Campaign' ?></title>

    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
    <!-- Bootstrap Icons -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css" rel="stylesheet">

    <style>
        :root {
            --nssf-primary: #145fa7;      /* Primary blue */
            --nssf-secondary: #0f1e45;    /* Dark blue */
            --nssf-accent: #8ec63e;       /* Lime green */
            --nssf-gold: #cda45e;         /* Gold accent */
            --nssf-light-blue: #2799d0;   /* Light blue */
            --nssf-light: #f8f9fa;        /* Light background */
        }

        body {
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            background-color: #f5f5f5;
        }

        .navbar {
            background: linear-gradient(135deg, var(--nssf-primary), var(--nssf-secondary));
        }

        .navbar-brand {
            font-weight: 700;
            letter-spacing: 1px;
            display: flex;
            align-items: center;
        }

        .navbar-brand img {
            height: 45px;
            margin-right: 12px;
        }

        .btn-nssf {
            background-color: var(--nssf-primary);
            border: none;
            color: white;
            transition: all 0.3s ease;
        }

        .btn-nssf:hover {
            background-color: var(--nssf-accent);
            color: var(--nssf-secondary);
        }

        .btn-nssf-outline {
            background-color: transparent;
            border: 2px solid var(--nssf-primary);
            color: var(--nssf-primary);
            transition: all 0.3s ease;
        }

        .btn-nssf-outline:hover {
            background-color: var(--nssf-primary);
            color: white;
        }

        .btn-nssf-gold {
            background-color: var(--nssf-gold);
            border: none;
            color: var(--nssf-secondary);
            transition: all 0.3s ease;
        }

        .btn-nssf-gold:hover {
            background-color: var(--nssf-accent);
            color: var(--nssf-secondary);
        }

        .card {
            border: none;
            border-radius: 12px;
            box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
            transition: all 0.3s ease;
            overflow: hidden;
        }

        .card:hover {
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.12);
        }

        .card-header {
            background: linear-gradient(135deg, var(--nssf-light) 0%, #ffffff 100%);
            border-bottom: 2px solid var(--nssf-primary);
            padding: 1rem 1.5rem;
            font-weight: 600;
        }

        .card-header h4,
        .card-header h5 {
            color: var(--nssf-primary);
        }

        .card-body {
            padding: 1.5rem;
        }

        /* Hero Card */
        .card-hero {
            background: linear-gradient(135deg, rgba(20, 95, 167, 0.9) 0%, rgba(15, 30, 69, 0.85) 100%),
                        url('/assets/img/money-matters-blue-geometric-finance-art_153608-60366.avif');
            background-size: cover;
            background-position: center;
            border-top: 4px solid var(--nssf-accent);
            position: relative;
        }

        .card-hero .card-body {
            padding: 4rem 2rem;
            position: relative;
            z-index: 1;
        }

        .card-hero h1,
        .card-hero p {
            color: white !important;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
        }

        .card-hero p.lead {
            color: rgba(255, 255, 255, 0.9) !important;
        }

        .card-hero .btn-nssf-outline {
            border-color: white;
            color: white;
        }

        .card-hero .btn-nssf-outline:hover {
            background-color: white;
            color: var(--nssf-primary);
        }

        .card-hero .btn-nssf {
            background-color: var(--nssf-accent);
            color: var(--nssf-secondary);
        }

        .card-hero .btn-nssf:hover {
            background-color: white;
            color: var(--nssf-primary);
        }

        /* Feature/Step Cards */
        .card-feature {
            border-top: 3px solid transparent;
            background: #ffffff;
        }

        .card-feature:hover {
            border-top-color: var(--nssf-accent);
            transform: translateY(-5px);
        }

        .card-feature .card-body {
            padding: 2rem 1.5rem;
        }

        .card-feature .display-4 {
            color: var(--nssf-primary);
            margin-bottom: 1rem;
        }

        .card-feature:hover .display-4 {
            color: var(--nssf-accent);
        }

        /* Info Cards */
        .card-info {
            border-left: 4px solid var(--nssf-primary);
        }

        .card-info .card-header {
            background: transparent;
            border-bottom: 1px solid rgba(0, 0, 0, 0.08);
        }

        /* Success Card */
        .card-success-result {
            border-top: 4px solid var(--nssf-accent);
            background: linear-gradient(180deg, #ffffff 0%, rgba(142, 198, 62, 0.05) 100%);
        }

        /* Review Cards (in form) */
        .card-review {
            background: var(--nssf-light);
            border-radius: 8px;
        }

        .card-review .card-header {
            background: var(--nssf-primary);
            color: white;
            border-bottom: none;
            padding: 0.75rem 1rem;
            font-size: 0.9rem;
        }

        .card-review .card-body {
            padding: 1rem;
        }

        /* Reference Number Card */
        .card-reference {
            background: linear-gradient(135deg, var(--nssf-primary) 0%, var(--nssf-secondary) 100%);
            color: white;
            border-radius: 10px;
        }

        .card-reference .card-body {
            padding: 1.5rem;
        }

        .card-reference .card-title {
            color: rgba(255, 255, 255, 0.8);
            font-size: 0.9rem;
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .card-reference .display-6 {
            color: white;
            font-family: monospace;
        }

        .main-content {
            flex: 1;
        }

        footer {
            background-color: var(--nssf-secondary);
            color: white;
            padding: 1.5rem 0;
            margin-top: auto;
        }

        footer a {
            color: var(--nssf-accent);
            text-decoration: none;
        }

        footer a:hover {
            color: var(--nssf-gold);
        }

        .form-step {
            display: none;
        }

        .form-step.active {
            display: block;
        }

        .step-indicator {
            display: flex;
            justify-content: space-between;
            margin-bottom: 2rem;
        }

        .step {
            flex: 1;
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 1rem;
            position: relative;
        }

        .step::after {
            content: '';
            position: absolute;
            top: calc(1rem + 20px);
            left: 50%;
            width: 100%;
            height: 2px;
            background-color: #dee2e6;
            z-index: 1;
        }

        .step:last-child::after {
            display: none;
        }

        .step .step-number {
            width: 40px;
            height: 40px;
            border-radius: 50%;
            background-color: #dee2e6;
            color: #6c757d;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 600;
            position: relative;
            z-index: 2;
        }

        .step.active .step-number,
        .step.completed .step-number {
            background-color: var(--nssf-primary);
            color: white;
        }

        .step.completed .step-number::after {
            content: '\f26b';
            font-family: 'bootstrap-icons';
        }

        .loading-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(255, 255, 255, 0.8);
            display: none;
            justify-content: center;
            align-items: center;
            z-index: 9999;
        }

        .loading-overlay.show {
            display: flex;
        }
    </style>

    <?= $this->renderSection('styles') ?>
</head>
<body>
    <!-- Navigation -->
    <nav class="navbar navbar-expand-lg navbar-dark">
        <div class="container">
            <a class="navbar-brand" href="<?= base_url() ?>">
                <img src="<?= base_url('assets/img/nssf-logo.png') ?>" alt="NSSF Logo">
                <span>NSSF Amnesty Campaign</span>
            </a>
        </div>
    </nav>

    <!-- Main Content -->
    <main class="main-content py-4">
        <div class="container">
            <?= $this->renderSection('content') ?>
        </div>
    </main>

    <!-- Footer -->
    <footer>
        <div class="container text-center">
            <p class="mb-0">&copy; <?= date('Y') ?> National Social Security Fund. All rights reserved.</p>
        </div>
    </footer>

    <!-- Loading Overlay -->
    <div class="loading-overlay" id="loadingOverlay">
        <div class="text-center">
            <div class="spinner-border" role="status" style="width: 3rem; height: 3rem; color: var(--nssf-primary);">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-3 text-muted">Please wait...</p>
        </div>
    </div>

    <!-- Bootstrap 5 JS Bundle -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
    <!-- jQuery -->
    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>

    <script>
        // Global loading functions
        function showLoading() {
            document.getElementById('loadingOverlay').classList.add('show');
        }

        function hideLoading() {
            document.getElementById('loadingOverlay').classList.remove('show');
        }

        // Toast notification
        function showToast(message, type = 'success') {
            const toastContainer = document.getElementById('toastContainer') || createToastContainer();
            const toastId = 'toast-' + Date.now();
            const bgClass = type === 'success' ? 'bg-success' : (type === 'error' ? 'bg-danger' : 'bg-warning');

            const toastHtml = `
                <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert">
                    <div class="d-flex">
                        <div class="toast-body">${message}</div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                </div>
            `;

            toastContainer.insertAdjacentHTML('beforeend', toastHtml);
            const toastElement = document.getElementById(toastId);
            const toast = new bootstrap.Toast(toastElement);
            toast.show();

            toastElement.addEventListener('hidden.bs.toast', () => toastElement.remove());
        }

        function createToastContainer() {
            const container = document.createElement('div');
            container.id = 'toastContainer';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
            return container;
        }
    </script>

    <?= $this->renderSection('scripts') ?>
</body>
</html>
