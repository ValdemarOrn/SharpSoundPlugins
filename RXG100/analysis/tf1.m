clc

R1 = 68000
R2 = 1000000
C1 = 0.01e-6
C2 = 250e-12
s = tf('s')

a = R1 + 1/(s*C1)
b = 1/(s*C2+1/R2)
h = b/(a+b)

% normalize numbers
%[num,den] = tfdata(h,'v');
%h = tf(num/den(1),den/den(1))

figure(1)
bode(h)
[num,den] = tfdata(h,'v');


[numd,dend] = bilinear(num,den,48000)
sys1 = tf(numd,dend,1/48000) 
figure(2)
bode(sys1)

[numd2,dend2] = bilinear(num,den,48000,10000)
sys2 = tf(numd2,dend2,1/48000) 
figure(3)
bode(sys2)

[fnum,fden] = tfdata(sys2,'v')

%[numd, demd] = bilinear(h,1000)