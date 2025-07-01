import axios from "axios";
import { getToken } from "../contexts/AuthContext";
import {
  cacheVehicles, getCachedVehicles,
  cacheReadings, getCachedReadings,
  enqueueRequest, flushQueue
} from "./offline";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "http://localhost:5000"
});

api.interceptors.request.use(async config => {
  const token = getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;

  if (!navigator.onLine && ["post","put","delete"].includes(config.method)) {
    await enqueueRequest({ url: config.url, method: config.method, body: config.data });
    return Promise.reject({ __queued: true, config });
  }
  return config;
});

api.interceptors.response.use(
  async response => {
    const { config, data } = response;
    if (config.method === "get") {
      if (config.url === "/api/vehicles") {
        await cacheVehicles(data);
      } else if (/^\/api\/vehicles\/\d+\/readings$/.test(config.url)) {
        await cacheReadings(data);
      }
    }
    return response;
  },
  async error => {
    const { config } = error;

    if (error.__queued) {
      return { data: null, status: 202, config };
    }

    if (!navigator.onLine && config.method === "get") {
      if (config.url === "/api/vehicles") {
        const vehicles = await getCachedVehicles();
        return { data: vehicles, status: 200, config };
      }

      // 2b) Lecturas filtradas por {vehicleId}
      const match = config.url.match(/^\/api\/vehicles\/(\d+)\/readings$/);
      if (match) {
        const vehicleId = Number(match[1]);
        let readings = await getCachedReadings();
        readings = readings.filter(r => Number(r.vehicleId) === vehicleId);
        const params = config.params || {};
        if (params.from) {
          const from = new Date(params.from);
          readings = readings.filter(r => new Date(r.recordedAt) >= from);
        }
        if (params.to) {
          const to = new Date(params.to);
          readings = readings.filter(r => new Date(r.recordedAt) <= to);
        }

        return { data: readings, status: 200, config };
      }
    }

    return Promise.reject(error);
  }
);

window.addEventListener("online", () => {
  flushQueue(api).catch(console.error);
});

export default api;
