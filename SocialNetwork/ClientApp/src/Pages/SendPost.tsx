import React from 'react';
import { Button, createStyles, makeStyles, Theme, CircularProgress } from '@material-ui/core';
import Config from '../Services/Config';
import { RequestManager } from '../Services/RequestManager';
import { useHistory } from 'react-router';

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            margin: theme.spacing(3)
        },
        sendArea: {
            display: 'block',
            width: '100%',
            minHeight: '150px',
            marginBottom: theme.spacing(2),
            fontSize: '12pt'
        },
        sendBtn: {
            minWidth: '100px',
            marginRight: theme.spacing(1),
        },
        line: {
            display: 'flex'
        }
    }));

export default function SendPost() {
    const classes = useStyles();
    let [charCount, setCharCount] = React.useState(0);
    const config = Config;
    const history = useHistory();
    return (
        <div className={classes.root}>
            <textarea name='text' className={classes.sendArea}
                onChange={(e) => {
                    if (e.currentTarget.value.length > 300)
                        e.currentTarget.value = e.currentTarget.value.substr(0, 300);
                    setCharCount(e.currentTarget.value.length / 3)
                }} />
            <div className={classes.line}>
                <Button variant='contained' color='primary' className={classes.sendBtn}
                    onClick={() => {
                        let reqManager = new RequestManager(config.apiBase);
                        reqManager.Post('/post/send', xhr => {
                            if (xhr.status === 200) {
                                history.push('/');
                            }
                        }, {
                            text: (document.getElementsByName('text')[0] as HTMLTextAreaElement).value
                        })
                    }}>
                    Send
                </Button>
                <CircularProgress value={charCount} color='primary' variant='static' />
            </div>
        </div>
    );
}