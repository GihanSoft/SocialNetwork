import { useEffect } from "react";
import { Navigate } from "react-router";

export default function SignOut() {
  useEffect(() => {
    localStorage.removeItem("AuthJWT");
  }, []);
  return <Navigate to="/" />;
}
