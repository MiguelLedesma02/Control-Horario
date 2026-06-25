/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        ink: { 950: '#0a0a0b', 900: '#111113', 800: '#1c1c1f', 700: '#2a2a2e', 600: '#3a3a3f' },
        accent: { DEFAULT: '#d4ff3a', muted: '#b8e02f' },
        warn: '#ffb547',
        bad: '#ff5470',
        good: '#3ddc97'
      },
      fontFamily: {
        display: ['"Space Grotesk"', 'system-ui', 'sans-serif'],
        body: ['"Inter"', 'system-ui', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'ui-monospace', 'monospace']
      }
    }
  },
  plugins: []
}
