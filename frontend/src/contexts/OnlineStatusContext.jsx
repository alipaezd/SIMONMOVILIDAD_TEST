import React, { createContext, useContext, useEffect, useState } from 'react';

const OnlineStatusContext = createContext(true);

export function OnlineStatusProvider({ children }) {
  const [online, setOnline] = useState(navigator.onLine);
  useEffect(() => {
    const update = () => setOnline(navigator.onLine);
    window.addEventListener('online',  update);
    window.addEventListener('offline', update);
    return () => {
      window.removeEventListener('online',  update);
      window.removeEventListener('offline', update);
    };
  }, []);
  return (
    <OnlineStatusContext.Provider value={online}>
      {children}
    </OnlineStatusContext.Provider>
  );
}

export function useOnlineStatus() {
  return useContext(OnlineStatusContext);
}
