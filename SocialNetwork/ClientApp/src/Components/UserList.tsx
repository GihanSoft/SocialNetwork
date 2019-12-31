import React from "react";
import { List, ListItem } from "@material-ui/core";
import User, { UserProps } from "./User";

let gSetUsers: React.Dispatch<React.SetStateAction<UserProps[] | undefined>>;

async function getUsersAsync(
    url: string,
    lastGuttedId?: number,
    maxCountToGet: number = 10,
    towardOlds: boolean = true
): Promise<UserProps[]> {
    let data = {
        lastGuttedId: lastGuttedId,
        maxCountToGet: maxCountToGet,
        towardOlds: towardOlds
    };
    let response = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("AuthJWT")}`
        },
        body: JSON.stringify(data)
    });
    let users: UserProps[] = await response.json();
    return users;
}

export default function UserList(props: { url: string }) {
    getUsersAsync(props.url).then(v => gSetUsers(v));
    return <UserListInternal url={props.url} />;
}

function UserListInternal(props: { url: string }) {
    let [users, setUsers] = React.useState<UserProps[]>();
    gSetUsers = setUsers;
    return (
        <>
            <List>
                {users?.map((user, index) => {
                    return (
                        <ListItem>
                            <User
                                key={index}
                                userName={user.userName}
                                isPrivate={user.isPrivate}
                                isFollowed={user.isFollowed}
                                isFollowAccepted={user.isFollowAccepted}
                                isFollowRequested={user.isFollowRequested}
                            />
                        </ListItem>
                    );
                })}
            </List>
        </>
    );
}
