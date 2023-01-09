#  MA3012RECEIVE configuration file

# DATA SOURCE Config
# Parameter for Connect
BindingIP		192.168.0.153	#default 0.0.0.0(Any) or 본 소프트웨어가 운영되는 localHost IP
BindingPort		12001	#수신 받고자 하는 Port 번호
RemoteIP		255.255.255.255 #default Broadcast(255.255.255.255) or  출발지 sourceIP (출발지 IP)

# Parameter for Data Checking
NETCode			DL	#NET  기관코드
StationCode		DY	#센서에 설정된 스테이션코드 (2자리)
LocationCode	A	#센서번호 (1자리), (참조-지하3개 중층2, 상층2개 설치된 경우: 지하층 센서(A,B,C) 중층 센서(M,N), 상층 (T,U)) (참조-지하1, 중1, 상1 설치된 경우 : 지하층(B), 중층(M), 상층(T)), 참조- 자유장(G)
ChannelName		HGE	HGN	HGZ	#(HGE, HGN, HGZ) Order 즉, 데이터 전송 순으로 기입. 반드시 3채널로 입력
ScaleFactor		3802.4 #카운터 값을 gal로 변환하기 위한 파라미터

MSEEDRecordig	true	#미니시드 저장 스레드 동작설정 (true: 사용, false: 미사용 ). 미니시드 저장여부

DBRecordig		true	#DB 저장 스레드 동작설정 (true: 사용, false: 미사용 ). 통합DB 데이터 저장 여부