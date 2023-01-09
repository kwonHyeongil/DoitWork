#  MA3012SOCK configuration file

# DATA SOURCE Config
# Parameter for Server Connect
BindingIP			192.168.0.153	#default 0.0.0.0(Any) or 본 소프트웨어가 운영되는 local IP
BindingPort			12001		# ma3012receive로 받는 수신포트번호 즉, 현장의 경우 센서의 PORT 번호
RemoteIP			255.255.255.255 #default Broadcast(255.255.255.255) or 원격지 sourceIP (출발지) 즉, 현장의 경우 센서의 IP
DirectConncetion 	true	#센서와 직접연결 여부, 현장의 경우 true, ma3012receive로 받은 경우 false

# Parameter for Data Checking
NETCode				DL
StationCode			NY	#센서에 설정된 스테이션코드 (2자리)
LocationCode		A	#센서번호 (1자리), (참조-지하3개 중층2, 상층2개 설치된 경우: 지하층 센서(A,B,C) 중층 센서(M,N), 상층 (T,U)) (참조-지하1, 중1, 상1 설치된 경우 : 지하층(B), 중층(M), 상층(T)), 참조- 자유장(G)


# 실시간 데이터 DataSender Parameter
# Destination    dest		IP(or Host)		Port
 Destination 	local 		192.168.0.153	12005  	#실시간데이터 전송 IP, PORT

# 이벤트 데이터 DataSender Parameter
# EvtDestination    dest		IP(or Host)		Port
#EvtDestination 	local 		192.168.0.153	12006  	#이벤트데이터 전송 IP, PORT