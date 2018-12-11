module.exports = {
  baseUrl: process.env.NODE_ENV === 'production' ? '/spa/dist/' : '/',
  devServer: {
    proxy: 'http://localhost:51299',
  },
};
