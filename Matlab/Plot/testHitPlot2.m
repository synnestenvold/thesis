clc; clear;

x = linspace(-5,5);
y = x.^3-12*x;
plot(x,y)

xt = [-2 2];
yt = [16 -16];
str = ['local max','local min'];
text(xt,yt,str)