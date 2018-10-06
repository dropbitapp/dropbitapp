module.exports = {
  baseUrl: process.env.NODE_ENV === 'production' ? '/spa/dist/' : '/',
  devServer: {
    proxy: {
      '*': {
        target: 'https://localhost:44363',
        changeOrigin: true,
      },
    },
  },
};
