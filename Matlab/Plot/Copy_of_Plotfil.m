clc; clear;


%figure(1)
figure('Renderer', 'painters', 'Position', [10 10 900 600])
hold on

set(gca, 'FontName', 'Times New Roman')
set(gca,'fontsize',10)
%legend([plot1,plot2],'t_{i}','t_{ri}','Location','southeast');
%axis tight
%grid minor

xaxis = [90 720 2430];
yaxis1 = [-8.85986
-9.26453
-9.33582];

yaxis2 = [-8.85986
-9.26453
-9.33582];


p1 = plot(xaxis,yaxis1,'b','LineWidth',1,'MarkerSize',4);
p2 = plot(xaxis,yaxis2,'r','LineWidth',1,'MarkerSize',4);


h = [p1(1),p2(1)];
legend(h,'Abaqus','SolidsVR');

ax = gca;
ax.XMinorTick = 'on';
ax.XAxis.TickValues = xaxis;

xlabel('Number of elements')
ylabel('Displacement[mm]')
title('Cantilever comparison')