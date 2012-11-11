//http://wiibrew.org/wiki/Wiimote/Extension_Controllers/Classic_Controller

#include <Wire.h>
#include <string.h>

#undef int
#include <stdio.h>

#include <AFMotor.h>

AF_DCMotor l_motor(3);
AF_DCMotor r_motor(4);


int photoReflector[2] = {A2,A3};
int sensorRanges[6] = {0}; // [0] = reflector1 MIN, [1] = reflector1 MAX, [2] = reflector1 MID
                           // [3] = reflector2 MIN, [4] = reflector2 MAX, [5] = reflector2 MID
                           
#define l_min = 5;
#define l_max = 57;
#define r_min = 3;
#define r_max = 29;

int l_mid =31;
int r_mid =16;

int l_y; 
int r_y;

int z_button;
int c_button;

int fullrevolutions[2] = {0};

boolean lastreadingHigh[2];
int halfrevolutions[2] = {0};
int quarterrevlotions[2] = {0};

void setup ()
{
  Serial.begin(9600);
  Serial.print("Finished setup\n");
  Wire.begin ();		// join i2c bus with address 0x52
  nunchuck_init (); // send the initilization handshake
  
  calibratePhotoReflectors();
}

void nunchuck_init ()
{
  Wire.beginTransmission (0x52);	// transmit to device 0x52
  Wire.write(0x40);		// sends memory address
  Wire.write(0x00);		// sends sent a zero.  
  Wire.endTransmission ();	// stop transmitting
}

void send_zero ()
{
  Wire.beginTransmission (0x52);	// transmit to device 0x52
  Wire.write(0x00);		// sends one byte
  Wire.endTransmission ();	// stop transmitting
}

void loop ()
{
  if (readNunchucky())
  {
//    if (l_mid==0) {
//      l_mid = l_y;
//      Serial.print (l_mid, DEC);
//       Serial.print ("\t");
//    }
//    
//    
//    if (r_mid==0) {
//      r_mid = r_y;
//      Serial.print (r_mid, DEC);
//      Serial.print ("\r\n");
//    }
    setMotorSpeed();

    readPhotoReflectors();
    
    boolean revchanged = false; 
     for (int i=0;i<2;i++) {
       int rev = halfrevolutions[i]/2;
        if (rev!=fullrevolutions[i])
        {
         fullrevolutions[i] = rev;
        revchanged = true;
           
          
        }
     }
     
     if (revchanged) {
      
      Serial.print (fullrevolutions[0], DEC);
            Serial.print ("\t"); 
      Serial.print (fullrevolutions[1], DEC);
       Serial.print ("\r\n"); 
     }
//    Serial.print (l_y, DEC);
//    Serial.print ("\t");
//  
//    Serial.print (r_y, DEC);
//    Serial.print ("\t");
//  
//    Serial.print ("\r\n");
      
   
  } 
  else { 
    //cant read nunchucky - stop motors just to be safe
      
    l_motor.run(RELEASE);
    r_motor.run(RELEASE);
    
    
  }
  delay (10);
}

// Print the input data we have recieved
// accel data is 10 bits long
// so we read 8 bits, then we have to add
// on the last 2 bits.  That is why I
// multiply them by 2 * 2
boolean readNunchucky()
{
  
  uint8_t outbuf[6]; // array to store arduino output
  int cnt = 0;

  Wire.requestFrom (0x52, 6);	// request data from nunchuck
  while (Wire.available ())
  {
    outbuf[cnt] = nunchuk_decode_byte (Wire.read());	// receive byte as an integer
    //digitalWrite (ledPin, HIGH);	// sets the LED on
    cnt++;
  }

  // If we recieved the 6 bytes, then go print them
  if (cnt >= 5)
  {
    
    l_y = outbuf[1] & (1<<6)-1; //only read first 6 bits
    r_y = outbuf[2] & (1<<5)-1; //only read first 5 bits  
    
    z_button = 0;
    c_button = 0;
  
   // byte outbuf[5] contains bits for z and c buttons
   // it also contains the least significant bits for the accelerometer data
   // so we have to check each bit of byte outbuf[5]
    if ((outbuf[5] >> 0) & 1)
      {
        z_button = 1;
      }
    if ((outbuf[5] >> 1) & 1)
      {
        c_button = 1;
      }

    send_zero (); // send the request for next bytes

    return true;
  }


  send_zero (); // send the request for next bytes

  return false;
}

void setMotorSpeed() {
  
  int l = l_y;
  int r = r_y; // precision on r is 1/2 of l
  
  l = l-l_mid; // mid = 0, down = -32, up = +32
  l = l*10; // up = 256;
  
  r = r-r_mid; // mid = 0, down = -16, up = +16
  r = r*10*2; // up = 256;
  
  if (l<0)
    l_motor.run(FORWARD);
  else
    l_motor.run(BACKWARD);
    
  if (r<0)
    r_motor.run(FORWARD);
  else
    r_motor.run(BACKWARD);
  
  l = min(abs(l),255);
  r = min(abs(r),255);
  
  if (l<30)
    l = 0;
    
  if (r<30)
    r = 0;
  
  l_motor.setSpeed(l);
  r_motor.setSpeed(r);
 
 
   
//    Serial.print (l, DEC);
//    Serial.print ("\t");
//  
//    Serial.print (r, DEC);
//    Serial.print ("\t");
//  
//    Serial.print ("\r\n");
      
 
}
// Encode data to format that most wiimote drivers except
// only needed if you use one of the regular wiimote drivers
char nunchuk_decode_byte (char x)
{
  x = (x ^ 0x17) + 0x17;
  return x;
}



void calibratePhotoReflectors() {
    
  sensorRanges[0] = 1023;
  sensorRanges[3] = 1023;
  int sensorValue;

  Serial.println("Calibrating");

  // turn on motor
  l_motor.setSpeed(200);
  l_motor.run(FORWARD);
  
  
  // turn on motor
  r_motor.setSpeed(200);
  r_motor.run(FORWARD); 
  
  unsigned long startMillis = millis();

   while (millis()-startMillis < 1000) {
        
      for (int i=0;i<2;i++) {
        sensorValue = analogRead(photoReflector[i]);
    
        // record the maximum sensor value
        if (sensorValue > sensorRanges[(i*3)+1]) {
          sensorRanges[(i*3)+1] = sensorValue;
        }
        else if (sensorValue < sensorRanges[(i*3)]) {
          sensorRanges[(i*3)] = sensorValue;
        }
            
      } 
   }
  sensorRanges[2] = (sensorRanges[0] + sensorRanges[1])/2;
  sensorRanges[5] = (sensorRanges[3] + sensorRanges[4])/2;
  
  
  l_motor.run(RELEASE);
  r_motor.run(RELEASE);
  
  Serial.print("Sensor 1 range: ");
  Serial.print(sensorRanges[0]);
  Serial.print(" - ");  
  Serial.println(sensorRanges[1]);  
  
  Serial.print("Sensor 2 range: ");
  Serial.print(sensorRanges[3]);
  Serial.print(" - ");  
  Serial.println(sensorRanges[4]);  

  
}

void readPhotoReflectors()
{
 
  int sensorValue;
  
  for (int i=0;i<2;i++) {
    sensorValue = analogRead(photoReflector[i]);
  
      if (sensorValue > sensorRanges[(i*3)+2]) {
         if (!lastreadingHigh[i]) {
           lastreadingHigh[i] = true;
           halfrevolutions[i]++;
           quarterrevlotions[i]++;
           
//          Serial.print("Half rev: ");
//          
//          Serial.print(halfrevolutions[0]);
//          
//          Serial.print("\t");
//          Serial.print(halfrevolutions[1]);
//          Serial.print("\r\n");
          
         }
      } else if (lastreadingHigh[i]) {
          lastreadingHigh[i]=false;
          quarterrevlotions[i]++;
      }
  
  }
   
  
  
}
