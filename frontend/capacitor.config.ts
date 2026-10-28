import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'cz.upce.nnpro.elearning',
  appName: 'E-learning',
  webDir: 'dist',
  server: {
    androidScheme: 'https'
  }
};

export default config;
