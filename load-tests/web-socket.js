import ws from 'k6/ws';
import { check, sleep } from 'k6';
import http from 'k6/http';
import { Counter } from 'k6/metrics';

const personCounterMetric = new Counter('person_events');

export const options = {
    vus: 100,
    duration: '120s',
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
        var hub = http.post(url);

        check(hub, {
            'Can negotiate hub': r => r.status == 200
        });

        var payload = hub.json();

        negotiateConnection(payload.url, payload.accessToken);
    }

    function negotiateConnection(url, accessToken){
        let negotiateUrl = url.replace('?hub', 'negotiate?hub');
        var azure = http.post(negotiateUrl, {}, {headers: {'Authorization': `Bearer ${accessToken}`}});

        check(azure, {
            'Can negotiate azure signal r': r => r.status == 200
        });

        var connectionInfo = azure.json();

        openWebSocket(url, connectionInfo.connectionId, accessToken);
    }

    function openWebSocket(url, connectionId, accessToken){
        let webSocketUrl = url.replace('https://', 'wss://');

        webSocketUrl = `${webSocketUrl}&id=${connectionId}&access_token=${accessToken}`;

        const res = ws.connect(webSocketUrl, null, socket => {
            socket.on('open', () => {

                check(connectionId, {
                    'Has connection id': r => !!r
                });

                socket.send(`{"protocol":"json","version":1}${endChar}`);
                socket.send(`{"type":6}${endChar}`);

                sleep(1);

                const added = http.post(`${baseUrl}api/connections/${connectionId}/person-events`,
                    JSON.stringify({trigger: 'Added'}),
                    { headers: { 'Content-Type': 'application/json' }});

                check(added, {
                    'Registered for added events': r => r.status == 200
                });

                sleep(1);

                const updated = http.post(`${baseUrl}api/connections/${connectionId}/person-events`,
                    JSON.stringify({trigger: 'Updated'}),
                    { headers: { 'Content-Type': 'application/json' }});

                check(updated, {
                    'Registered for updated events': r => r.status == 200
                });

                sleep(1);

                http.get(`${baseUrl}api/entity-pump`);
            });

            let personEventCounter = 0;

            socket.on('message', event => {
                event.split(endChar).forEach(socketData => {
                    if(!socketData){
                        return;
                    }
                    let data = JSON.parse(socketData);

                    if(data && data.type && data.type === 1 && data.target && data.target){
                        switch(data.target){
                            case 'personEvent':
                                personEventCounter++;
                                personCounterMetric.add(1);
                                break;
                        }
                    }
                });
            });

            socket.on('close', () => {
                check(personEventCounter, {
                    'Received at least one entity message': r => r > 0
                });
            });

            socket.setInterval(() => {
                socket.close();
            }, 1000 * 10);
        });

        check(res, { 'web socket status is 101': (r) => r && r.status === 101 });
    }

    sleep(1);

    var userNameResponse = http.get(`${baseUrl}api/username`);

    check(userNameResponse, {
        'Can get username': r => r.status == 200
    });

    const userName = userNameResponse.body;

    const group = userName.split('_')[0];

    sleep(1);

    connectToSignalR(userName, group);
}