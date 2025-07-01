// src/services/offline.js
import { openDB } from 'idb';

const DB_NAME    = 'simonmovilidad';
const DB_VERSION = 2;             
const STORE_VEH  = 'vehicles';
const STORE_READ = 'readings';
const STORE_Q    = 'queue';

async function getDb() {
  return openDB(DB_NAME, DB_VERSION, {
    upgrade(db, oldVersion) {
      if (oldVersion < 2) {
        if (db.objectStoreNames.contains(STORE_VEH)) {
          db.deleteObjectStore(STORE_VEH);
        }
        db.createObjectStore(STORE_VEH, { keyPath: 'id' });

        if (db.objectStoreNames.contains(STORE_READ)) {
          db.deleteObjectStore(STORE_READ);
        }
        db.createObjectStore(STORE_READ, { autoIncrement: true });

        if (db.objectStoreNames.contains(STORE_Q)) {
          db.deleteObjectStore(STORE_Q);
        }
        db.createObjectStore(STORE_Q, { autoIncrement: true });
      }
    }
  });
}

export async function cacheVehicles(arr) {
  const db = await getDb();
  const tx = db.transaction(STORE_VEH, 'readwrite');
  arr.forEach(v => tx.store.put(v));
  await tx.done;
}

export async function getCachedVehicles() {
  const db = await getDb();
  return db.getAll(STORE_VEH);
}

export async function cacheReadings(arr) {
  const db = await getDb();
  const tx = db.transaction(STORE_READ, 'readwrite');
  arr.forEach(r => tx.store.put(r));
  await tx.done;
}

export async function getCachedReadings() {
  const db = await getDb();
  return db.getAll(STORE_READ);
}

export async function enqueueRequest(req) {
  const db = await getDb();
  const tx = db.transaction(STORE_Q, 'readwrite');
  await tx.store.add(req);
  await tx.done;
}

export async function flushQueue(api) {
  const db = await getDb();
  const tx = db.transaction(STORE_Q, 'readwrite');
  const store   = tx.store;
  const entries = await store.getAll();
  const keys    = await store.getAllKeys();
  for (let i = 0; i < entries.length; i++) {
    const { url, method, body } = entries[i];
    try {
      await api.request({ url, method, data: body });
      await store.delete(keys[i]);
    } catch (e) {
      console.error("Error reenviando petición encolada:", e);
    }
  }
  await tx.done;
}
