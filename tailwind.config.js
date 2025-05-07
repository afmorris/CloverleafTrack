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
                accent: '#67c379'
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

