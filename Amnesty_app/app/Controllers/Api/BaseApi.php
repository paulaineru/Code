<?php

namespace App\Controllers\Api;

use CodeIgniter\RESTful\ResourceController;

class BaseApi extends ResourceController
{
    protected $format = 'json';

    /**
     * Constructor - verify API key
     */
    public function __construct()
    {
        // API key verification can be done in a filter instead
    }

    /**
     * Verify API key from header
     */
    protected function verifyApiKey(): bool
    {
        $apiKey = $this->request->getHeaderLine('X-API-Key');
        $validKey = getenv('API_KEY') ?: 'your-secret-api-key';

        return $apiKey === $validKey;
    }

    /**
     * Return unauthorized response
     */
    protected function unauthorized(string $message = 'Unauthorized')
    {
        return $this->respond([
            'status'  => 401,
            'error'   => $message,
            'message' => 'Invalid or missing API key'
        ], 401);
    }

    /**
     * Return not found response
     */
    protected function notFound(string $message = 'Resource not found')
    {
        return $this->respond([
            'status'  => 404,
            'error'   => $message
        ], 404);
    }

    /**
     * Return success response with data
     */
    protected function success($data, string $message = 'Success')
    {
        return $this->respond([
            'status'  => 200,
            'message' => $message,
            'data'    => $data
        ]);
    }

    /**
     * Return error response
     */
    protected function error(string $message, int $statusCode = 400)
    {
        return $this->respond([
            'status'  => $statusCode,
            'error'   => $message
        ], $statusCode);
    }
}
