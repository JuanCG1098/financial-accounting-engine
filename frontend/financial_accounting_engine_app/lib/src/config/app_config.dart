/// Application-wide configuration.
///
/// The API base URL is provided at build/run time via `--dart-define=API_BASE_URL=...`
/// and defaults to the local Docker setup.
class AppConfig {
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://localhost:8080',
  );
}
