/*
Liquid flow rate sensor -
Adapted code from IYhacking.com Arvind Sanjeev by Ntokozo Hunter Motsumi
 */

byte statusLed = 13;
byte nextPin = 4;
byte sensorInterrupt = 0;  // 0 = digital pin 2
byte sensorPin = 2;
byte sensorInterrupt2 = 1;  // 0 = digital pin 2
byte sensorPin2 = 3;
// The hall-effect flow sensor outputs approximately 4.5 pulses per second per
// litre/minute of flow.
float calibrationFactor = 4.5;
volatile byte pulseCount;
volatile byte pulseCount2;   
float flowRate;
unsigned int flowMilliLitres;
unsigned long totalMilliLitres;

unsigned long oldTime;

const byte numPins = 5;
byte pins[] = { 12, 11, 10, 9, 8, 7};

void setup()
{
  // Initialize a serial connection for reporting values to the host
  Serial.begin(9600);
   
  // Set up the status LED line as an output
  pinMode(statusLed, OUTPUT);
  pinMode(nextPin, INPUT);
  for (int i=0; i < 6; i++){
    pinMode(pins[i], OUTPUT); 
  }
  digitalWrite(statusLed, HIGH);  // We have an active-low LED attached 
  pinMode(sensorPin, INPUT);
  digitalWrite(sensorPin, HIGH);
  pinMode(sensorPin2, INPUT);
  digitalWrite(sensorPin2, HIGH);
  pulseCount = 0;
  pulseCount2 = 0;  
  flowRate = 0.0;
  flowMilliLitres = 0;
  totalMilliLitres = 0;
  oldTime = 0;
  // The Hall-effect sensor is connected to pin 2 which uses interrupt 0.
  // Configured to trigger on a FALLING state change (transition from HIGH
  // state to LOW state)
  attachInterrupt(sensorInterrupt, pulseCounter, FALLING);
  attachInterrupt(sensorInterrupt2, pulseCounter2, FALLING);
  
}

/**
 * Main program loop
 */
void loop()
{
  //gets request from IoT device to switch to another sensors
  int val = digitalRead(nextPin);  
  if(val==0){
    sensorWork(0,sensorInterrupt); 
  }else{
    sensorWork(1,sensorInterrupt2); 
  }  
}

//method to get readings from sensor
void sensorWork(int senseFlag, int interrupt){
  if((millis() - oldTime) > 1000)    // Only process counters once per second
  { 
    // Disable the interrupt while calculating flow rate and sending the value to
    // the host
    detachInterrupt(interrupt); 
    // Because this loop may not complete in exactly 1 second intervals we calculate
    if (senseFlag ==1){
       flowRate = ((1000.0 / (millis() - oldTime)) * pulseCount2) / calibrationFactor;
    }else{
       flowRate = ((1000.0 / (millis() - oldTime)) * pulseCount) / calibrationFactor;
    }
    oldTime = millis(); 
    flowMilliLitres = (flowRate / 60) * 1000; 
    // Add the millilitres passed in this second to the cumulative total
    totalMilliLitres += flowMilliLitres;      
    unsigned int frac;
    String myStr;
    int zeros = String(int(flowRate),BIN).length();
    for (int i=0; i < 6 - zeros; i++) {//This will add zero to string as need
      myStr = myStr + "0";
    }
    myStr = myStr + String(int(flowRate),BIN);
    Serial.print(int(myStr[0])-48);
    digitalWrite(pins[0], int(myStr[0])-48);
    Serial.print(int(myStr[1])-48);
    digitalWrite(pins[1], int(myStr[1])-48);
    Serial.print(int(myStr[2])-48);
    digitalWrite(pins[2], int(myStr[2])-48);
    Serial.print(int(myStr[3])-48);
    digitalWrite(pins[3], int(myStr[3])-48);
    Serial.print(int(myStr[4])-48);
    digitalWrite(pins[4], int(myStr[4])-48);
    Serial.print(int(myStr[5])-48);
    digitalWrite(pins[5], int(myStr[5])-48); 
    Serial.println();
    // Print the flow rate for this second in litres / minute
    Serial.print("Flow rate: ");
    Serial.print(int(flowRate));  // Print the integer part of the variable
    Serial.print("L/min");
    Serial.print("\t");       // Print tab space
    // Print the cumulative total of litres flowed since starting
    Serial.print("Output Liquid Quantity: ");        
    Serial.print(totalMilliLitres);
    Serial.println("mL"); 
    Serial.print("\t");       // Print tab space
    Serial.print(totalMilliLitres/1000);
    Serial.print("L");
    Serial.println();
    // Reset the pulse counter so we can start incrementing again
    pulseCount = 0;
    pulseCount2 = 0;   
    // Enable the interrupt again now that we've finished sending output
    if (senseFlag ==1){
       attachInterrupt(interrupt, pulseCounter2, FALLING);
    }else{
       attachInterrupt(interrupt, pulseCounter, FALLING);
    }    
  }
}

/*
Insterrupt Service Routine
 */
void pulseCounter()
{
  // Increment the pulse counter
  pulseCount++;
}
void pulseCounter2()
{
  // Increment the pulse counter
  pulseCount2++;
}