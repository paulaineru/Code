<?php

namespace App\Commands;

use CodeIgniter\CLI\BaseCommand;
use CodeIgniter\CLI\CLI;
use App\Libraries\SyncService;

class SyncRun extends BaseCommand
{
    protected $group       = 'Sync';
    protected $name        = 'sync:run';
    protected $description = 'Process pending syncs to remote server';
    protected $usage       = 'sync:run [options]';
    protected $arguments   = [];
    protected $options     = [
        '--limit' => 'Maximum number of syncs to process (default: 50)',
    ];

    public function run(array $params)
    {
        $limit = (int) ($params['limit'] ?? CLI::getOption('limit') ?? 50);

        CLI::write('Starting sync process...', 'yellow');
        CLI::write("Processing up to {$limit} pending syncs", 'light_gray');
        CLI::newLine();

        $syncService = new SyncService();
        $results = $syncService->processPendingSyncs($limit);

        CLI::write('Sync process completed!', 'green');
        CLI::newLine();

        CLI::write('Results:', 'white');
        CLI::write("  Processed: {$results['processed']}", 'light_gray');
        CLI::write("  Succeeded: {$results['succeeded']}", 'green');
        CLI::write("  Failed:    {$results['failed']}", $results['failed'] > 0 ? 'red' : 'light_gray');

        if ($results['failed'] > 0) {
            CLI::newLine();
            CLI::write('Some syncs failed. They will be retried on the next run with exponential backoff.', 'yellow');
        }

        return $results['failed'] > 0 ? 1 : 0;
    }
}
