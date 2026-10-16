import axios from 'axios';


// Shared Axios client for requests to the stock transfer API.

const API = axios.create({
  baseURL: "http://localhost:5135/api",
});

export default API;
