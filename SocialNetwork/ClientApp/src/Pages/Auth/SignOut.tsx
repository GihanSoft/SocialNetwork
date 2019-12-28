import React from "react";
import { useHistory } from "react-router-dom";

export default function SignOut() {
    localStorage.removeItem('AuthJWT');
    let history = useHistory();
    history.push('/')
    return (
        <></>
    );
}
