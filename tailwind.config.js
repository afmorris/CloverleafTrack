/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
      './CloverleafTrack.Web/Views/**/*.cshtml',
      './CloverleafTrack.Web/wwwroot/js/**/*.js'
  ],
  theme: {
    extend: {
        colors: {
            ctf: {
                nav: '#233625',
                footer: '#e7f5e7',
                accent: '#7ac943',
                darkaccent: '#3b7f2e'
            }
        },
        fontFamily: {
            heading: ['Montserrat', 'sans-serif'],
            body: ['Inter', 'sans-serif'],
        },
    },
  },
    darkMode: 'class',
  plugins: [],
}

