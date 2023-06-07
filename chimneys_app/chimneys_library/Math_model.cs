using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace chimneys_library
{
    public class Math_model
    {
        double h      ; //Высота участка, м
        double d_vn_s ; //Внутренний диаметр в начале участка, м
        double d_vn_f ; //Внутренний диаметр в конце участка, м
        double l1     ; //Толщина слоя внутренней футеровки, м
        double l2     ; //Толщина воздушного зазора, м 
        double l3     ; //Толщина слоя теплоизоляции, м
        double l4     ; //Толщина слоя прижимной футеровки, м
        double l5     ; //Толщина ствола, м
        double y1     ; //Коэффициент теплопроводности слоя внутренней футеровки, Вт/(м*К)
        double y2     ; //Коэффициент теплопроводности  слоя теплоизоляции, Вт/(м*К)
        double y3     ; //Коэффициент теплопроводности прижимной футеровки, Вт/(м*К)
        double y4     ; //Коэффициент теплопроводности ствола, Вт/(м*К)
        double L      ; //Расход при нормальных условиях, куб.м./с
        double T      ; //Температура  при входе в трубу,  С
        double C_co2  ; //Содержание СО2 
        double C_h2o  ; //Содержание Н2О
        double T_okr  ; //Температура окружающего воздуха,  С
        double V      ; //Скорость ветра, м/с
        double h_2    ; //высота 11 участка

        public Math_model(ChimneysInput input) //конструктор класса
        {
            h = input.h;
            d_vn_s = input.d_vn_s;
            d_vn_f = input.d_vn_f;
            l1 = input.l1;
            l2 = input.l2;
            l3 = input.l3;
            l4 = input.l4;
            l5 = input.l5;
            y1 = input.y1;
            y2 = input.y2;
            y3 = input.y3;
            y4 = input.y4;
            L = input.L;
            T = input.T;
            C_co2 = input.C_co2;
            C_h2o = input.C_h2o;
            T_okr = input.T_okr;
            V = input.V;
            h_2 = input.h_2;
        }

        public ChimneysOut calc()
        {

            double V_average_1 = 0;
            double a_konv = 0;
            double a_lk = 0;
            double a_sum = 0;
            double a_nar = 0;
            double Line_p = 0;
            double T_average = 0;
            double T_in_fut = 0;
            double T_outside_futer = 0;
            double T_in_fut_prig = 0;
            double T_in_cladk = 0;
            double T_out_cladk = 0;
            
            double line_R1;
            double logics;
            double logics1;
            double line_R2;
            double line_R3;
            double line_R4;
            double podg_T_gases = 1;
            double podg_T_in_fut = 1;
            double podg_T_outside_futer = 1;
            double podg_T_in_fut_prig = 1;
            double T_after = T - 1;//2
            double T_average_inside = T - 5;//4
            double T_outside_fut = T - 10; //26
            double T_inside_kladki = T - 20;//27
            while ((podg_T_gases > 0.0003) || (podg_T_in_fut > 0.0006) || (podg_T_outside_futer > 0.0001) || (podg_T_in_fut_prig > 0.0001))
            {

                T_average = (T_after + T) / 2;//3
                double d_average = (d_vn_s + d_vn_f) / 2;//6
                V_average_1 = 4 * L / (3.14 * Pow(d_average, 2));//7
                double V_average_2 = V_average_1 * (1 + T_average / 273);//8
                double N_flue_gases = Pow(10, -7) * T_average + 122 * Pow(10, -7);//9
                double Pr_flue_gases = (-3 * Pow(10, -10) * Pow(T_average, 3)) + (6 * Pow(10, -7) * Pow(T_average, 2)) - (5 * Pow(10, -4) * T_average) + 0.7414;
                double y_flue_gases = (0.8598 * T_average + 227) / 10000;//11
                double Re = V_average_2 * d_average / N_flue_gases;//12
                double Nu = 0.021 * Pow(Re, 0.8) * Pow(Pr_flue_gases, 0.43);//13
                a_konv = Nu * y_flue_gases / d_vn_s;//14
                double kl_gases = (0.8 + (1.6 * C_h2o / 100)) * (1 - 0.00038 * (T_average + 273)) / Sqrt(((C_h2o + C_co2) / 100) * d_average);//15
                double E_gases = 1 - Exp(-((C_co2 + C_h2o) / 100) * kl_gases * d_average);//16
                double kl_wall = (0.8 + 1.6 * C_h2o / 100) * (1 - 0.00038 * (T_average_inside + 273)) / Sqrt(((C_h2o + C_co2) / 100) * d_average);//17
                double A = 1 - Exp(-((C_h2o + C_co2) / 100) * kl_wall * d_average);//18
                double E_pr = 5.67 / (1 / 0.8 + 1 / A - 1);//19
                double T_gases_skobk = (E_gases / A) * Pow(((T_average + 273) / 100), 4);//20
                double T_wall_skobk = Pow(((T_average_inside + 273) / 100), 4);//21
                a_lk = E_pr * (T_gases_skobk - T_wall_skobk) / (T_average - T_average_inside);//22
                a_sum = a_konv + a_lk;//23
                double line_R = 1 / (a_sum * d_average);//24
                if (l1 == 0)
                    line_R1 = 0;
                else
                    line_R1 = (1 / (2 * y1)) * Log((d_average + 2 * l1) / d_average); //25
                double T_average_air = (T_outside_fut + T_inside_kladki) / 2;//28
                double y_air = (0.0683 * T_average_air + 24.68) / 1000;//29
                double N_air = (0.1167 * T_average_air + 12.32) / Pow(10, 6); //30
                double Gr = 9.81 * (T_outside_fut - T_inside_kladki) * Pow(l2, 3) / (273 * Pow(N_air, 2));//31
                double Pr_air = -3 * Pow(10, -7) * Pow(T_average_air, 2) - 0.0001 * T_average_air + 0.705;//32
                double Gr_Pr_air = Gr * Pr_air; //33
                if (Gr_Pr_air < Pow(10, 7))
                    logics = 0.105 * Pow(Gr_Pr_air, 0.3);
                else
                    logics = 0.4 * Pow(Gr_Pr_air, 0.2);//34
                if (Gr_Pr_air > 1000)
                    logics1 = 0.18 * Pow(Gr_Pr_air, 0.25);
                else
                    logics1 = 1;//35
                double y_ekv_air = logics * y_air;//36
                if (l2 == 0)
                    line_R2 = 0;
                else
                    line_R2 = (1 / (2 * y_ekv_air)) * Log((d_average + 2 * l1 + 2 * l2) / (d_average + 2 * l1));//37
                if (l3 == 0)
                    line_R3 = 0;
                else
                    line_R3 = (1 / (2 * y2)) * Log((d_average + 2 * l1 + 2 * l3) / (d_average + 2 * l1));//38
                if (l4 == 0)
                    line_R4 = 0;
                else
                    line_R4 = (1 / (2 * y3)) * Log((d_average + 2 * l1 + 2 * l2 + 2 * l3 + 2 * l4) / (d_average + 2 * l1 + 2 * l2 + 2 * l3));//39
                double line_R5 = (1 / (2 * y4)) * Log((d_average + 2 * l1 + 2 * l2 + 2 * l3 + 2 * l4 + 2 * l5) / (d_average + 2 * l1 + 2 * l2 + 2 * l3 + 2 * l4));//40

                if (h_2 >= 200)
                    a_nar = 6.3 * Pow(2.6 * V, 0.66);
                else if (h_2 >= 150)
                    a_nar = 6.3 * Pow(2.3 * V, 0.66);
                else if (h_2 >= 100)
                    a_nar = 6.3 * Pow(2.1 * V, 0.66);
                else if (h_2 >= 50)
                    a_nar = 6.3 * Pow(1.6 * V, 0.66);
                else if (h_2 >= 25)
                    a_nar = 6.3 * Pow(1.2 * V, 0.66);
                else
                    a_nar = 6.3 * Pow(V, 0.66);//43
                double R_yd = 1 / (a_nar * (d_average + 2 * l1 + 2 * l2 + 2 * l3 + 2 * l4 + 2 * l5));//44
                double Line_Q = 1 / (line_R + line_R1 + line_R2 + line_R3 + line_R4 + line_R5 + R_yd);//45
                Line_p = Line_Q * 3.14 * (T_average - T_okr);//46
                double C_gases = 0.2721 * T_average + 1041.4;//47
                double T_pad = Line_p / (L * C_gases);//48
                double T_pad1 = T_pad * h;//49
                double T_after1 = T - T_pad1;//50
                podg_T_gases = T_after1 - T_after;//51
                T_in_fut = T_average - (Line_p / (3.14 * a_sum * d_average));//52
                podg_T_in_fut = T_in_fut - T_average_inside;//53
                T_outside_futer = T_average - ((Line_p / 3.14) * (line_R + line_R1));//54
                podg_T_outside_futer = T_outside_futer - T_outside_fut;//55
                T_in_fut_prig = T_average - ((Line_p / 3.14) * (line_R + line_R1 + line_R2 + line_R3));//56
                podg_T_in_fut_prig = T_in_fut_prig - T_inside_kladki;//57
                T_in_cladk = T_average - ((Line_p / 3.14) * (line_R + line_R1 + line_R2 + line_R3 + line_R4));//58
                T_out_cladk = T_okr + (Line_p / (3.14 * a_nar * (d_average + 2 * l1 + 2 * l2 + 2 * l5)));//59
                double prov_T_out_cladk = T_average - ((Line_p / 3.14) * (line_R + line_R1 + line_R2 + line_R3 + line_R4 + line_R5));//60
                                                                                                                                    
                T_after = T_after1;
                T_average_inside = T_in_fut;
                T_outside_fut = T_outside_futer;
                T_inside_kladki = T_in_fut_prig;
            }
            T = T_after;
            return new ChimneysOut
            {
                T               = T,
                V_average_1     = V_average_1,
                a_konv          = a_konv,
                a_lk            = a_lk,
                a_sum           = a_sum,
                a_nar           = a_nar,
                Line_p          =  Line_p,
                T_average       = T_average,
                T_in_fut        = T_in_fut,
                T_outside_fut   = T_outside_futer,
                T_in_fut_prig   = T_in_fut_prig,
                T_in_cladk      = T_in_cladk,
                T_out_cladk     = T_out_cladk,

            };
        }
    }
}
