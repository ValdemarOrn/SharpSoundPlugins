% Stage 1 - Hipass með Fc = 16 Hz


%% Gain TF function

%s = tf('s');

gain = 0.5;

R1 = 100e3*gain
R2 = 3.3e3
c1 = 100e-12
c2 = 0.047e-6

b = 1 + (c1*R1 + c2*(R1 + R2))*s + c1*c2*R1*R2*s^2
a = 1 + (c1*R1 + c2*R2)*s + c1*c2*R1*R2*s^2

tf = b/a

bode(tf)

%% High pass filter eftir gain

%s = tf('s');

gain = 0.01;

R1 = 100e3*(1-gain)
R2 = 8.2e3
c1 = 0.068e-6

b = c1*R2*s
a = 1 + c1*(R1 + R2)*s

tf = b/a

bode(tf)

% Hard saturate, smá lowpass filter á eftir með fc = 18Khz !!
% Hipass fyrir clipper, Fc = 40Hz
% Tonestack
% Opamp circuit, sama transfer function og fyrir gainið, nema aðrar tölur

%% Contour control

%s = tf('s');

R1 = 100;
R2 = 33e3;
R3 = 33e3;
c1 = 0.1e-6;
c2 = 0.047e-6;
c3 = 0.22e-6;
c4 = 0.001e-6;
RO = 100e3
P1 = 50e3
P2 = 50e3

b4 = c3*(c1*c2*c4*P1*R2*R3 + c1*c2*c4*P1*P2*(R2 + R3))
b3 = c3*(c1*c2*P1*P2 + c2*c4*R2*R3 + c1*c4*P1*(R2 + R3) + c2*c4*P2*(R2 + R3))
b2 = c3*(c1*P1 + c2*P2 + c4*(R2 + R3))
b1 = c3;
b0 = 0;

b = b0 + b1*s + b2*s^2 + b3*s^3 + b4*s^4

a4 = c1* c2* c3* c4* (P1* R1* (R2* R3 + P2* (R2 + R3)) + (P1* (P2 + R1)* R2 + R1* R2* R3 + P1* (P2 + R1 + R2)* R3 + P2* R1* (R2 + R3))* RO)
a3 = (c2* c3* c4* (R1* R2* R3 + R2* R3* RO + R1* (R2 + R3)* RO +  P2* (R2 + R3)* (R1 + RO)) + c1* (c3* c4* (R2 + R3)* (R1* RO + P1* (R1 + RO)) +  c2* (c4 *(P2*R1 + P1* (P2 + R1))* R2 + c4* (R1* (P2 + R2) + P1* (P2 + R1 + R2))* R3 +  c3* (R1* (R2* (R3 + RO) + P2* (R2 + R3 + RO)) + P1* ((R1 + R2)* (R3 + RO) + P2 *(R1 + R2 + R3 + RO))))))
a2 = (c3* c4* (R2 + R3)* (R1 + RO) + c1* (c2* (R1* (P2 + R2) + P1* (P2 + R1 + R2)) + c4* (P1 + R1)* (R2 + R3) +c3* (R1* (R2 + R3 + RO) + P1* (R1 + R2 + R3 + RO))) + c2* (c4* (P2 + R1)* R2 + c4* (P2 + R1 + R2)* R3 + c3* ((R1 + R2)* (R3 + RO) + P2* (R1 + R2 + R3 + RO)))) 
a1 = (c1*P1 + c2*P2 + c1*R1 + c2*R1 + c3*R1 + c2*R2 + c3*R2 + c4*R2 + c3*R3 + c4*R3 + c3*RO)
a0 = 1

a = a0 + a1*s + a2*s^2 + a3*s^3 + a4*s^4

% ÞETTA ER CURRENT


tf = b/a * (RO+1/(s*c3));
bode(tf)

% Hipass, 15Hz, Lowpass 1.56Khz, en mætti hækka í 10Khz til að fá meira
% edgy sound, algengt mod