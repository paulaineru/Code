<?php

use CodeIgniter\Router\RouteCollection;

/**
 * @var RouteCollection $routes
 */

// Home routes
$routes->get('/', 'Home::index');
$routes->get('download-template', 'Home::downloadTemplate');
$routes->post('accept-terms', 'Home::acceptTerms');
$routes->get('check-terms', 'Home::checkTerms');

// Application routes
$routes->get('apply', 'Application::form');
$routes->post('apply/upload-sat', 'Application::uploadSat');
$routes->post('apply/upload-payment', 'Application::uploadPayment');
$routes->post('apply/submit', 'Application::submit');
$routes->get('apply/success/(:segment)', 'Application::success/$1');
$routes->get('apply/status/(:segment)', 'Application::checkStatus/$1');

// API routes
$routes->group('api', ['namespace' => 'App\Controllers\Api'], function ($routes) {
    // Applications
    $routes->get('applications', 'Applications::index');
    $routes->get('applications/(:num)', 'Applications::show/$1');
    $routes->put('applications/(:num)/status', 'Applications::updateStatus/$1');

    // SAT Data
    $routes->get('applications/(:num)/sat-summary', 'SatData::summary/$1');
    $routes->get('applications/(:num)/sat-monthly', 'SatData::monthly/$1');
    $routes->get('applications/(:num)/sat-arrears', 'SatData::arrears/$1');

    // Files
    $routes->get('applications/(:num)/files/(:alpha)', 'Applications::downloadFile/$1/$2');

    // Sync
    $routes->post('sync/(:num)', 'Applications::triggerSync/$1');
});
