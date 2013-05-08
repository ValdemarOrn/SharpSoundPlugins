clc
clear all

% To estimate the frequency selectivity of the 0.47uF capacitor

s = tf('s')
Fc = 48000;

ttt = db2mag(-23.5) + db2mag(-9.35)*(s)/(860*2*pi+s)
figure(1)
bode(ttt);
% 
[num,den] = tfdata(ttt,'v')
% 
[numd, dend] = bilinear(num,den,Fc)

figure(2)
ttt2 = tf(numd,dend,1/Fc);
bode(ttt2);