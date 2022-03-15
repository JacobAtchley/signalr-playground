import ws from 'k6/ws';
import { check, sleep } from 'k6';
import http from 'k6/http';

export const options = {
    vus: 1000,
    duration: '30s',
    thresholds: {
      'http_req_duration{status:200}': ['max>=0'],
      'http_req_duration{status:500}': ['max>=0'],
    },
    'summaryTrendStats': ['min', 'med', 'avg', 'p(90)', 'p(95)', 'max', 'count'],
  };

export default function () {

    const endChar = String.fromCharCode(30);
    const baseUrl = 'https://localhost:7212/';

    function connectToSignalR(userName, group){
        const url = `${baseUrl}chat/negotiate?userName=${userName}&group=${group}&negotiateVersion=1`;
        var payload = http.post(url).json();
        negotiateConnection(payload.url, payload.accessToken);
    }

    function negotiateConnection(url, accessToken){
        let negotiateUrl = url.replace('?hub', 'negotiate?hub');

        var connectionInfo = http.post(negotiateUrl, {}, {headers: {'Authorization': `Bearer ${accessToken}`}}).json();
        openWebSocket(url, connectionInfo.connectionId, accessToken);
    }

    function openWebSocket(url, connectionId, accessToken){
        let webSocketUrl = url.replace('https://', 'wss://');

        webSocketUrl = `${webSocketUrl}&id=${connectionId}&access_token=${accessToken}`;

        const res = ws.connect(webSocketUrl, null, socket => {
            socket.on('open', () => {
                //console.log('connected');
                socket.send(`{"protocol":"json","version":1}${endChar}`);
                socket.send(`{"type":6}${endChar}`);

                socket.setInterval(() => {
                    socket.close();
                }, 1000 * 10);
            });
            //socket.on('close', () => console.log('disconnected'));
            socket.on('message', event => {
                //console.log('message', event);
            });
        });

        check(res, { 'status is 101': (r) => r && r.status === 101 });
    }

    var userName = http.get(`${baseUrl}api/username`).body;
    const group = userName.split('_')[0];
    sleep(2);
    connectToSignalR(userName, group);
}