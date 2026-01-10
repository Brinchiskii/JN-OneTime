import axios from "axios";

const https = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "https://onetimeapi20260107194346-bdf4effqafd3d7c3.northeurope-01.azurewebsites.net/api"
  // baseURL: "https://localhost:5273/api"
});

export default https;