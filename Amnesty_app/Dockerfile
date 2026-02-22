# =============================================================================
# Stage 1: Composer Dependencies
# =============================================================================
FROM composer:2.6 AS composer-deps

WORKDIR /app

# Copy composer files first for better layer caching
COPY composer.json composer.lock ./

# Install dependencies without dev dependencies for production
# --ignore-platform-reqs because extensions are installed in production stage
RUN composer install --no-dev --no-scripts --no-autoloader --prefer-dist --ignore-platform-reqs

# Copy application source
COPY . .

# Generate optimized autoloader
RUN composer dump-autoload --optimize --classmap-authoritative --ignore-platform-reqs

# =============================================================================
# Stage 2: Production Image
# =============================================================================
FROM php:8.2-fpm-alpine AS production

# Set working directory
WORKDIR /var/www/html

# Install system dependencies
RUN apk add --no-cache \
    # Required for intl extension
    icu-dev \
    icu-libs \
    # Required for GD extension
    freetype-dev \
    libjpeg-turbo-dev \
    libpng-dev \
    libwebp-dev \
    # Required for zip extension
    libzip-dev \
    # Required for xml extensions
    libxml2-dev \
    # Required for curl
    curl-dev \
    # Utilities
    bash \
    # MySQL client for healthchecks
    mysql-client \
    # Supervisor for running multiple processes
    supervisor \
    # Cron for scheduled tasks
    dcron \
    # Required for healthcheck (pgrep)
    procps \
    # Required for connection check
    netcat-openbsd

# Install PHP extensions
RUN docker-php-ext-configure gd \
        --with-freetype \
        --with-jpeg \
        --with-webp \
    && docker-php-ext-install -j$(nproc) \
        intl \
        mysqli \
        pdo \
        pdo_mysql \
        zip \
        gd \
        opcache \
        fileinfo

# Copy custom PHP configuration
COPY docker/php/php.ini /usr/local/etc/php/conf.d/custom.ini
COPY docker/php/opcache.ini /usr/local/etc/php/conf.d/opcache.ini
COPY docker/php/php-fpm.conf /usr/local/etc/php-fpm.d/www.conf

# Copy supervisor configuration
COPY docker/supervisor/supervisord.conf /etc/supervisord.conf

# Copy cron configuration
COPY docker/cron/crontab /etc/crontabs/www-data

# Create necessary directories with proper permissions
RUN mkdir -p \
    /var/www/html/writable/cache \
    /var/www/html/writable/logs \
    /var/www/html/writable/session \
    /var/www/html/writable/uploads \
    /var/www/html/writable/debugbar \
    /var/log/supervisor

# Copy application from composer stage
COPY --from=composer-deps --chown=www-data:www-data /app /var/www/html

# Copy entrypoint script
COPY docker/docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

# Set proper permissions
RUN chown -R www-data:www-data /var/www/html \
    && chmod -R 755 /var/www/html/public \
    && chmod -R 775 /var/www/html/writable

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD php-fpm-healthcheck || exit 1

# Expose PHP-FPM port
EXPOSE 9000

# Entry point
ENTRYPOINT ["docker-entrypoint.sh"]
CMD ["supervisord", "-n", "-c", "/etc/supervisord.conf"]

# =============================================================================
# Stage 3: Development Image
# =============================================================================
FROM production AS development

# Install development dependencies
RUN apk add --no-cache \
    vim \
    nano \
    htop \
    git

# Install Xdebug for debugging
RUN apk add --no-cache --virtual .build-deps $PHPIZE_DEPS \
    && pecl install xdebug \
    && docker-php-ext-enable xdebug \
    && apk del .build-deps

# Copy Xdebug configuration
COPY docker/php/xdebug.ini /usr/local/etc/php/conf.d/xdebug.ini

# Enable opcache timestamp validation for development
RUN echo "opcache.validate_timestamps=1" >> /usr/local/etc/php/conf.d/opcache.ini \
    && echo "opcache.revalidate_freq=0" >> /usr/local/etc/php/conf.d/opcache.ini

# Install Composer in development image
COPY --from=composer:2.6 /usr/bin/composer /usr/bin/composer
