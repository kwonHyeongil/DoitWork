import os
import clr
import numpy
import obspy
import time
from datetime import datetime
from obspy import UTCDateTime, read, Trace, Stream

def creatFoloder(directory):
    try:
        if not (os.path.exists(directory)):
            os.makedirs(directory)
    except OSError as er:
        print('Error : Creating directory. ' + er)
        
def PathJoin(str1, str2):
    return os.path.join(str1, str2)

def TANKREMOVE(str1, str2, str3):
    flag = True
    try:
        if(os.path.exists(str1)):
            os.remove(str1)
    except:
        flag = False
        pass
    try:
        if(os.path.exists(str2)):
            os.remove(str2)
    except:
        flag = False
        pass
    try:
        if(os.path.exists(str3)):
            os.remove(str3)
    except:
        flag = False
        pass
    return flag
        
class MSEED:
    def __init__(self, StationCode, OBSCode, Location, Channel, Npts, SamplingHz, StartTime, XData, YData, ZData, EW_HOME, EW_PARAMS, MSEEDSavePath,TANKSavePath):
        self.StationCode = StationCode;
        self.OBSCode = OBSCode;
        self.Location = Location;
        self.Channel = Channel;
        self.Npts = Npts;
        self.SamplingHz = SamplingHz;
        self.StartTime = StartTime;
        self.XData = numpy.array(XData);
        self.YData = numpy.array(YData);
        self.ZData = numpy.array(ZData);
        self.strTime = datetime.strptime(StartTime, '%Y-%m-%d %H:%M:%S')
        self.EW_HOME = EW_HOME;
        self.EW_PARAMS = EW_PARAMS;
        self.MSEEDSavePath = MSEEDSavePath;
        self.TANKSavePath = TANKSavePath;

    def MSWrite(self):
        x_stats= {'network': self.StationCode ,
        'station': self.OBSCode,
        'location': self.Location,
        'channel': self.Channel[0],
        'npts': self.Npts,
        'sampling_rate': self.SamplingHz,
        'mseed' : {'dataquality' : 'D'},
        'starttime': UTCDateTime(self.StartTime)}

        y_stats= {'network': self.StationCode ,
        'station': self.OBSCode,
        'location': self.Location,
        'channel': self.Channel[1],
        'npts': self.Npts,
        'sampling_rate': self.SamplingHz,
        'mseed' : {'dataquality' : 'D'},
        'starttime': UTCDateTime(self.StartTime)}

        z_stats= {'network': self.StationCode ,
        'station': self.OBSCode,
        'location': self.Location,
        'channel': self.Channel[2],
        'npts': self.Npts,
        'sampling_rate': self.SamplingHz,
        'mseed' : {'dataquality' : 'D'},
        'starttime': UTCDateTime(self.StartTime)}

        creatFoloder(self.MSEEDSavePath)
        creatFoloder(self.TANKSavePath)
        
        MS2TANK = 'ms2tank.exe'
        recoringFlag = False
        try:
            st_x = Stream()
            st_y = Stream()
            st_z = Stream()
            strTANKEXE_PATH = PathJoin(self.EW_HOME,MS2TANK)

            strMSEED_PATH_X = PathJoin(self.MSEEDSavePath, self.StationCode + '_' + self.OBSCode+self.Location + '_' + self.Channel[0] + '.mseed')
            strTANKFILE_PATH_X = PathJoin(self.TANKSavePath,self.StationCode + '_' + self.OBSCode+self.Location + '_' + self.Channel[0] + '.tank')
            strMSEED_PATH_Y = PathJoin(self.MSEEDSavePath, self.StationCode + '_' + self.OBSCode+self.Location + '_' + self.Channel[1] + '.mseed')
            strTANKFILE_PATH_Y = PathJoin(self.TANKSavePath,self.StationCode + '_' + self.OBSCode+self.Location + '_' + self.Channel[1] + '.tank')
            strMSEED_PATH_Z = PathJoin(self.MSEEDSavePath, self.StationCode + '_' + self.OBSCode+self.Location + '_' + self.Channel[2] + '.mseed')
            strTANKFILE_PATH_Z = PathJoin(self.TANKSavePath,self.StationCode + '_' + self.OBSCode+self.Location + '_' + self.Channel[2] + '.tank')

            st_x = Stream([Trace(data=self.XData, header=x_stats)])
            st_x.merge(method = 1, fill_value='interpolate', interpolation_samples = -1)
            st_x.write(strMSEED_PATH_X,format='MSEED',encoding='11',reclen=512)

            st_y = Stream([Trace(data=self.YData, header=y_stats)])
            st_y.merge(method = 1, fill_value='interpolate', interpolation_samples = -1)
            st_y.write(strMSEED_PATH_Y,format='MSEED',encoding='11',reclen=512)

            st_z = Stream([Trace(data=self.ZData, header=z_stats)])
            st_z.merge(method = 1, fill_value='interpolate', interpolation_samples = -1)
            st_z.write(strMSEED_PATH_Z,format='MSEED',encoding='11',reclen=512)
            
            tank_remove_ck = TANKREMOVE(strTANKFILE_PATH_X,strTANKFILE_PATH_Y,strTANKFILE_PATH_Z)
            loop_num = 0
            while(tank_remove_ck == False and loop_num < 3):
                tank_remove_ck = TANKREMOVE(strTANKFILE_PATH_X,strTANKFILE_PATH_Y,strTANKFILE_PATH_Z)
                if(tank_remove_ck == True):
                    break
                else:
                    loop_num += 1
                    time.sleep(1)
            
            os.system(strTANKEXE_PATH +" -n 1000 " + strMSEED_PATH_X +' >> ' + strTANKFILE_PATH_X)
            os.system(strTANKEXE_PATH +" -n 1000 " + strMSEED_PATH_Y +' >> ' + strTANKFILE_PATH_Y)
            os.system(strTANKEXE_PATH +" -n 1000 " + strMSEED_PATH_Z +' >> ' + strTANKFILE_PATH_Z)
            time.sleep(2)
            recoringFlag = True
        except Exception as er:
            recoringFlag = False
        finally:
            return recoringFlag
            

